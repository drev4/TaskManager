using MediatR;

namespace TaskMgr.Api.Application.Projects.Commands.DeleteProject;

public record DeleteProjectCommand : IRequest<bool>
{
    public Guid ProjectId { get; init; }
    public Guid UserId { get; init; }
}
