using MediatR;
using TaskMgr.Api.Application.DTOs;

namespace TaskMgr.Api.Application.Tasks.Queries.GetTasks;

public record GetTasksQuery : IRequest<PagedResponseDto<TaskItemDto>>
{
    public Guid UserId { get; init; }
    public TaskFilterDto? Filter { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
