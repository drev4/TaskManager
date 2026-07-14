using MediatR;

namespace TaskMgr.Api.Application.Tasks.Commands.DeleteTask;

public record DeleteTaskCommand : IRequest<bool>
{
    public Guid TaskId { get; init; }
    public Guid UserId { get; init; }
}
