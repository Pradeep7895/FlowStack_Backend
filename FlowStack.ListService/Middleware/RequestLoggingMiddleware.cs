public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    private static readonly HashSet<string> _skip = new(StringComparer.OrdinalIgnoreCase)
        { "/health", "/swagger", "/swagger/v1/swagger.json" };

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (_skip.Any(s => path.StartsWith(s, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";
        var userId = context.User.FindFirst("sub")?.Value ?? "anonymous";

        _logger.LogInformation(
            "-> [{CorrelationId}] {Method} {Path}{Query} | User: {UserId}",
            correlationId, context.Request.Method,
            context.Request.Path, context.Request.QueryString, userId);

        await _next(context);
        sw.Stop();

        var status = context.Response.StatusCode;
        var level  = status >= 500 ? LogLevel.Error : status >= 400 ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(level,
            "<- [{CorrelationId}] {Method} {Path} -> {StatusCode} ({Duration}ms)",
            correlationId, context.Request.Method,
            context.Request.Path, status, sw.ElapsedMilliseconds);
    }
}
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}