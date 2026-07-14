using MediatR;
using TaskMgr.Api.Application.DTOs;

namespace TaskMgr.Api.Application.Projects.Commands.CreateProject;

public record CreateProjectCommand : IRequest<ProjectDto>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Color { get; init; }
    public DateTime? DueDate { get; init; }
    public Guid UserId { get; init; }
}
