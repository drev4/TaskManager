using MediatR;
using TaskMgr.Api.Application.DTOs;

namespace TaskMgr.Api.Application.Tasks.Queries.GetTaskById;

public record GetTaskByIdQuery : IRequest<TaskItemDto?>
{
    public Guid TaskId { get; init; }
    public Guid UserId { get; init; }
}
