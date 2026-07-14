using MediatR;
using TaskMgr.Api.Application.DTOs;

namespace TaskMgr.Api.Application.Users.Commands.UpdateUserProfile;

public record UpdateUserProfileCommand : IRequest<UserDto>
{
    public Guid UserId { get; init; }
    public string? DisplayName { get; init; }
    public string? Avatar { get; init; }
    public string? PreferredLanguage { get; init; }
}
