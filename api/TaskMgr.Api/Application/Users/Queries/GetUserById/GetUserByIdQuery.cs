using MediatR;
using TaskMgr.Api.Application.DTOs;

namespace TaskMgr.Api.Application.Users.Queries.GetUserById;

public record GetUserByIdQuery : IRequest<UserDto?>
{
    public Guid UserId { get; init; }
}
