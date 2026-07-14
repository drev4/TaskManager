using MediatR;
using TaskMgr.Api.Application.DTOs;

namespace TaskMgr.Api.Application.Users.Commands.InitializeUser;

public record InitializeUserCommand : IRequest<InitializeUserResult>
{
    public string AzureObjectId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}

public record InitializeUserResult(UserDto User, bool IsNew);
