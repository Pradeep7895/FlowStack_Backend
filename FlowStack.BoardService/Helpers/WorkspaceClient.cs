namespace FlowStack.BoardService.Helpers;

// Board creation requires confirming the requester is a member
// of the target workspace — this client makes that check.
// Registered as a typed client in Program.cs via IHttpClientFactory.
public class WorkspaceClient
{
    private readonly HttpClient _http;
    private readonly ILogger<WorkspaceClient> _logger;

    public WorkspaceClient(HttpClient http, ILogger<WorkspaceClient> logger)
    {
        _http   = http;
        _logger = logger;
    }

    /// Calls GET /api/workspaces/{workspaceId}/members/{userId}/check
    /// Returns true if the user is a member of the workspace.
    /// Returns false on any error to fail safe — board creation is blocked.
    public async Task<bool> IsMemberOfWorkspaceAsync(Guid workspaceId, Guid userId)
    {
        try
        {
            var response = await _http.GetAsync(
                $"/api/workspaces/{workspaceId}/members/{userId}/check");

            if (!response.IsSuccessStatusCode) return false;

            var body = await response.Content.ReadFromJsonAsync<WorkspaceMemberCheckResponse>();
            return body?.IsMember ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Workspace membership check failed for workspace {WorkspaceId} user {UserId}. " +
                "Denying board creation to fail safe.",
                workspaceId, userId);
            return false;
        }
    }

    // Returns true if the user is an Admin or Owner of the workspace.
    // Used when creating a Private board — only workspace admins can do this.
    public async Task<bool> IsAdminOfWorkspaceAsync(Guid workspaceId, Guid userId)
    {
        try
        {
            var response = await _http.GetAsync(
                $"/api/workspaces/{workspaceId}/members/{userId}/check");

            if (!response.IsSuccessStatusCode) return false;

            var body = await response.Content.ReadFromJsonAsync<WorkspaceMemberCheckResponse>();
            return body?.IsAdminOrOwner ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Workspace admin check failed for workspace {WorkspaceId} user {UserId}.",
                workspaceId, userId);
            return false;
        }
    }

    private class WorkspaceMemberCheckResponse
    {
        public bool IsMember { get; set; }
        public bool IsAdminOrOwner { get; set; }
    }
}