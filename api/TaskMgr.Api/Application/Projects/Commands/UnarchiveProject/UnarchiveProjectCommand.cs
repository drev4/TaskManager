using MediatR;

namespace TaskMgr.Api.Application.Projects.Commands.UnarchiveProject;

public record UnarchiveProjectCommand : IRequest<bool>
{
    public Guid ProjectId { get; init; }
    public Guid UserId { get; init; }
}
