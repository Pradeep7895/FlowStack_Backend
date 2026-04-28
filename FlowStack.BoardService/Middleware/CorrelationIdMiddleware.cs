namespace FlowStack.BoardService.Middleware;


// Assigns a unique CorrelationId to every incoming request.

// This is essential in a microservices architecture:
//   React → board-service (correlationId: abc123)
//         → workspace-service (same correlationId: abc123 forwarded)
//   All logs across all services share the same ID — one search finds everything.
public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Re-use existing correlation ID from upstream (gateway / other service)
        // or generate a new one for this request
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing)
                            && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        // Store in Items so all middleware and controllers can access it
        context.Items["CorrelationId"] = correlationId;

        // Echo back in response header so client can correlate
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(HeaderName, correlationId);
            return Task.CompletedTask;
        });

        await _next(context);
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}