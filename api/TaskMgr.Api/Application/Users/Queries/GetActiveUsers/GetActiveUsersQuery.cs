using MediatR;
using TaskMgr.Api.Application.DTOs;

namespace TaskMgr.Api.Application.Users.Queries.GetActiveUsers;

public record GetActiveUsersQuery : IRequest<List<UserDto>>;
