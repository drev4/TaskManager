namespace TaskMgr.Api.Infrastructure.Authorization;

public static class AuthorizationPolicies
{
    /// <summary>
    /// Requires an authenticated user when Auth:Enabled=true; always succeeds when
    /// Auth:Enabled=false, preserving the mock-user local dev experience.
    /// </summary>
    public const string ConditionalAuth = "ConditionalAuth";
}
