using System.Security.Claims;
using FlowStack.AuthService.Helpers;

namespace FlowStack.AuthService.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RequestLogStore _logStore;

    public RequestLoggingMiddleware(RequestDelegate next, RequestLogStore logStore)
    {
        _next = next;
        _logStore = logStore;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? context.User?.FindFirst("sub")?.Value;

        var entry = new RequestLogEntry
        {
            Method = context.Request.Method,
            Path = context.Request.Path,
            StatusCode = context.Response.StatusCode,
            UserId = userId,
            IPAddress = context.Connection.RemoteIpAddress?.ToString()
        };

        _logStore.AddLog(entry);
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
