using System.Net;
using System.Text.Json;

namespace FlowStack.BoardService.Middleware;

// Error mapping:
//   UnauthorizedAccessException → 403 Forbidden
//   KeyNotFoundException        → 404 Not Found
//   InvalidOperationException   → 400 Bad Request
//   ArgumentException           → 400 Bad Request
//   Everything else             → 500 Internal Server Error

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next   = next;
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
                "Unhandled exception. CorrelationId: {CorrelationId} | " +
                "{Method} {Path} | {ExceptionType}: {ExceptionMessage}",
                context.Items["CorrelationId"]?.ToString() ?? "N/A",
                context.Request.Method,
                context.Request.Path,
                ex.GetType().Name,
                ex.Message);

            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        // Must set before writing — once body starts, status code is locked
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, ex.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        var correlationId = context.Items["CorrelationId"]?.ToString() ?? string.Empty;

        var body = JsonSerializer.Serialize(new
        {
            status = (int)statusCode,
            error = statusCode.ToString(),
            message,
            correlationId  
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(body);
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}