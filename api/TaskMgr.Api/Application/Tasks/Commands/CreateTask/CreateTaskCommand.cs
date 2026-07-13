using MediatR;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Enums;

namespace TaskMgr.Api.Application.Tasks.Commands.CreateTask;

public record CreateTaskCommand : IRequest<TaskItemDto>
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid ProjectId { get; init; }
    public TaskPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public decimal? EstimatedHours { get; init; }
    public List<string> Tags { get; init; } = new();
    public Guid? AssignedToUserId { get; init; }
    public Guid UserId { get; init; } // The user performing the action
}
