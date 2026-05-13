namespace FlowStack.LabelService.Helpers;

public class TaskClient
{
    private readonly HttpClient _http;
    private readonly ILogger<TaskClient> _logger;

    public TaskClient(HttpClient http, ILogger<TaskClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<Guid?> GetBoardIdForCardAsync(Guid cardId, string authHeader)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/cards/{cardId}");
            request.Headers.Add("Authorization", authHeader);

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("task-service returned {StatusCode} for card {CardId}", response.StatusCode, cardId);
                return null;
            }

            var card = await response.Content.ReadFromJsonAsync<CardDto>();
            return card?.BoardId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get card {CardId} from task-service.", cardId);
            return null;
        }
    }

    private class CardDto
    {
        public Guid BoardId { get; set; }
    }
}
