using MediatR;
using TaskMgr.Api.Application.DTOs;

namespace TaskMgr.Api.Application.Projects.Queries.GetProjects;

public record GetProjectsQuery : IRequest<List<ProjectDto>>
{
    public Guid UserId { get; init; }
    public bool IncludeArchived { get; init; }
}
