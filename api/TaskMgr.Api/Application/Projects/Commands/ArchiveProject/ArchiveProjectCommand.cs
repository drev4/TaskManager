using MediatR;

namespace TaskMgr.Api.Application.Projects.Commands.ArchiveProject;

public record ArchiveProjectCommand : IRequest<bool>
{
    public Guid ProjectId { get; init; }
    public Guid UserId { get; init; }
}
