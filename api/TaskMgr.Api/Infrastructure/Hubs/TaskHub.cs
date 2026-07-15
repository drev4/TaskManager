using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TaskMgr.Api.Application.Users.Queries.GetCurrentUser;
using TaskMgr.Api.Infrastructure.Authorization;
using TaskMgr.Api.Options;

namespace TaskMgr.Api.Infrastructure.Hubs;

[Authorize(Policy = AuthorizationPolicies.ConditionalAuth)]
public class TaskHub : Hub
{
    // Bob Wilson's ID from seed data - matches the mock-user fallback used by the
    // REST controllers so notifications sent to Group($"User_{OwnerUserId}") reach
    // this connection in local dev (Auth:Enabled=false).
    private const string MockUserId = "2C6D7627-738C-4D39-9644-E7B703332DC5";

    private readonly ISender _sender;
    private readonly IOptionsMonitor<AuthOptions> _authOptions;

    public TaskHub(ISender sender, IOptionsMonitor<AuthOptions> authOptions)
    {
        _sender = sender;
        _authOptions = authOptions;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = await GetCurrentUserIdAsync();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = await GetCurrentUserIdAsync();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Resolves the internal (domain) User.Id so group membership matches the
    /// OwnerUserId/AssignedToUserId that TaskCreatedEventHandler notifies. This is
    /// deliberately not Context.UserIdentifier, which is the raw Azure Object ID
    /// claim and does not equal the internal User.Id.
    /// </summary>
    private async Task<string> GetCurrentUserIdAsync()
    {
        if (!_authOptions.CurrentValue.Enabled)
        {
            return MockUserId;
        }

        var azureObjectId = Context.User?.FindFirst("oid")?.Value
                           ?? Context.User?.FindFirst("sub")?.Value
                           ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(azureObjectId))
        {
            // [Authorize] already guarantees an authenticated principal here, so this
            // should be unreachable; fail closed to a group nothing will ever target.
            return "unknown";
        }

        var user = await _sender.Send(new GetCurrentUserQuery { AzureObjectId = azureObjectId });
        return user?.Id.ToString() ?? azureObjectId;
    }
}
