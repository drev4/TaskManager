using MediatR;
using TaskMgr.Api.Application.DTOs;

namespace TaskMgr.Api.Application.Tasks.Commands.RecordTaskTime;

public record RecordTaskTimeCommand : IRequest<TaskItemDto?>
{
    public Guid TaskId { get; init; }
    public Guid UserId { get; init; }
    public decimal Hours { get; init; }
    public string? Description { get; init; }
}
