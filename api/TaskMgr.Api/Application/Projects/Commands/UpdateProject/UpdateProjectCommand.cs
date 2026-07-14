using MediatR;
using TaskMgr.Api.Application.DTOs;

namespace TaskMgr.Api.Application.Projects.Commands.UpdateProject;

public record UpdateProjectCommand : IRequest<ProjectDto?>
{
    public Guid ProjectId { get; init; }
    public Guid UserId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Color { get; init; }
    public DateTime? DueDate { get; init; }
}
