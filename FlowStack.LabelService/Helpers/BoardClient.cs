namespace FlowStack.LabelService.Helpers;

public class BoardClient
{
    private readonly HttpClient _http;
    private readonly ILogger<BoardClient> _logger;

    public BoardClient(HttpClient http, ILogger<BoardClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<BoardAccessResult> GetBoardAccessAsync(Guid boardId, Guid userId)
    {
        try
        {
            var response = await _http.GetAsync($"/api/boards/{boardId}/access/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "board-service returned {StatusCode} for board {BoardId} user {UserId}",
                    response.StatusCode, boardId, userId);
                return BoardAccessResult.Denied();
            }

            var result = await response.Content.ReadFromJsonAsync<BoardAccessResult>();
            return result ?? BoardAccessResult.Denied();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Board access check failed for board {BoardId} user {UserId}. Denying access to fail safe.",
                boardId, userId);
            return BoardAccessResult.Denied();
        }
    }
}

public class BoardAccessResult
{
    public bool IsMember { get; set; }
    public bool IsAdminOrCreator { get; set; }
    public bool IsObserver { get; set; }
    public bool IsClosed { get; set; }
    public Guid WorkspaceId { get; set; }

    public static BoardAccessResult Denied() => new(); // all false
}
