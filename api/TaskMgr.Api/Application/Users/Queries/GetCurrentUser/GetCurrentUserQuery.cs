using MediatR;
using TaskMgr.Api.Application.DTOs;

namespace TaskMgr.Api.Application.Users.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<UserDto?>
{
    public string AzureObjectId { get; init; } = string.Empty;
}
