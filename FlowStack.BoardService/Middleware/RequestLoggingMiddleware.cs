namespace FlowStack.BoardService.Middleware;

// Logs every incoming HTTP request and its response.
// Captures: method, path, query string, status code, duration, and user identity.
//
// Placed AFTER GlobalExceptionMiddleware and AFTER UseAuthentication
// so HttpContext.User is already populated with JWT claims.
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    // Paths that should not be logged — health checks and swagger are too noisy
    private static readonly HashSet<string> _skipPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/swagger",
        "/swagger/v1/swagger.json"
    };

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip logging for health checks and swagger
        if (_skipPaths.Any(skip => path.StartsWith(skip, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var stopwatch     = System.Diagnostics.Stopwatch.StartNew();
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";

        // Read user identity from JWT claims
        var userId   = context.User.FindFirst("sub")?.Value ?? "anonymous";
        var userRole = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                    ?? "none";

        _logger.LogInformation(
            "-> REQUEST  [{CorrelationId}] {Method} {Path}{Query} | User: {UserId} Role: {Role}",
            correlationId,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            userId,
            userRole);

        await _next(context);

        stopwatch.Stop();

        // Choose log level based on status code
        var statusCode = context.Response.StatusCode;
        if (statusCode >= 500)
        {
            _logger.LogError(
                "<- RESPONSE [{CorrelationId}] {Method} {Path} -> {StatusCode} ({Duration}ms)",
                correlationId, context.Request.Method,
                context.Request.Path, statusCode, stopwatch.ElapsedMilliseconds);
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(
                "<- RESPONSE [{CorrelationId}] {Method} {Path} -> {StatusCode} ({Duration}ms)",
                correlationId, context.Request.Method,
                context.Request.Path, statusCode, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogInformation(
                "<- RESPONSE [{CorrelationId}] {Method} {Path} -> {StatusCode} ({Duration}ms)",
                correlationId, context.Request.Method,
                context.Request.Path, statusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}