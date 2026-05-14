using System.Collections.Concurrent;

namespace FlowStack.AuthService.Helpers;

public class RequestLogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? UserId { get; set; }
    public string? IPAddress { get; set; }
}

public class RequestLogStore
{
    private readonly ConcurrentQueue<RequestLogEntry> _logs = new();
    private const int MaxLogs = 500;

    public void AddLog(RequestLogEntry entry)
    {
        _logs.Enqueue(entry);
        while (_logs.Count > MaxLogs)
        {
            _logs.TryDequeue(out _);
        }
    }

    public IEnumerable<RequestLogEntry> GetLogs()
    {
        return _logs.OrderByDescending(l => l.Timestamp).ToList();
    }
}
