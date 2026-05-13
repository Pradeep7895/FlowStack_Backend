using System.Net;
using System.Text.Json;

namespace FlowStack.Gateway.Middleware;

public class CorrelationIdMiddleware
{
    private const string Header = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var id = context.Request.Headers.TryGetValue(Header, out var existing)
                    && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = id;
        // Forward to downstream services
        context.Request.Headers[Header] = id;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(Header, id);
            return Task.CompletedTask;
        });
        await _next(context);
    }
}

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Gateway unhandled exception. CorrelationId: {CorrelationId} | {Method} {Path}",
                context.Items["CorrelationId"]?.ToString() ?? "N/A",
                context.Request.Method,
                context.Request.Path);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                status = 500,
                error = "InternalServerError",
                message = "An unexpected error occurred in the gateway.",
                correlationId = context.Items["CorrelationId"]?.ToString() ?? string.Empty
            }));
        }
    }
}

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    private static readonly HashSet<string> _skip =
        new(StringComparer.OrdinalIgnoreCase) { "/health", "/swagger" };

    public RequestLoggingMiddleware(RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
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
            "→ GATEWAY [{CorrelationId}] {Method} {Path} | User: {UserId}",
            correlationId, context.Request.Method, context.Request.Path, userId);

        await _next(context);
        sw.Stop();

        var status = context.Response.StatusCode;
        var level = status >= 500 ? LogLevel.Error
                    : status >= 400 ? LogLevel.Warning
                    : LogLevel.Information;

        _logger.Log(level,
            "← GATEWAY [{CorrelationId}] {Method} {Path} → {Status} ({Duration}ms)",
            correlationId, context.Request.Method,
            context.Request.Path, status, sw.ElapsedMilliseconds);
    }
}

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();

    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}