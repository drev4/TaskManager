using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using TaskMgr.Api.Options;

namespace TaskMgr.Api.Infrastructure.Authorization;

public class ConditionalAuthHandler : AuthorizationHandler<ConditionalAuthRequirement>
{
    private readonly IOptionsMonitor<AuthOptions> _authOptions;

    public ConditionalAuthHandler(IOptionsMonitor<AuthOptions> authOptions)
    {
        _authOptions = authOptions;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ConditionalAuthRequirement requirement)
    {
        if (!_authOptions.CurrentValue.Enabled || context.User.Identity?.IsAuthenticated == true)
        {
            context.Succeed(requirement);
        }

        // When neither condition holds, leave the requirement unsatisfied without
        // calling context.Fail(): ASP.NET Core's default authorization result handler
        // then challenges via the JWT bearer scheme (401) instead of forbidding (403).
        return Task.CompletedTask;
    }
}
