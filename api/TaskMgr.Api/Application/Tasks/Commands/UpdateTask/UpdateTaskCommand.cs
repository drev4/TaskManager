using MediatR;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Enums;

namespace TaskMgr.Api.Application.Tasks.Commands.UpdateTask;

public record UpdateTaskCommand : IRequest<TaskItemDto?>
{
    public Guid TaskId { get; init; }
    public Guid UserId { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public Domain.Enums.TaskStatus? Status { get; init; }
    public TaskPriority? Priority { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public DateTime? DueDate { get; init; }
    public decimal? EstimatedHours { get; init; }
    public List<string>? Tags { get; init; }
}
