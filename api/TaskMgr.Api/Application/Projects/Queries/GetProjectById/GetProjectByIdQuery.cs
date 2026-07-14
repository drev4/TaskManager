using MediatR;
using TaskMgr.Api.Application.DTOs;

namespace TaskMgr.Api.Application.Projects.Queries.GetProjectById;

public record GetProjectByIdQuery : IRequest<ProjectDto?>
{
    public Guid ProjectId { get; init; }
    public Guid UserId { get; init; }
    public bool IncludeTasks { get; init; }
}
