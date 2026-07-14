using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediatR;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Application.Projects.Commands.ArchiveProject;
using TaskMgr.Api.Application.Projects.Commands.CreateProject;
using TaskMgr.Api.Application.Projects.Commands.DeleteProject;
using TaskMgr.Api.Application.Projects.Commands.UnarchiveProject;
using TaskMgr.Api.Application.Projects.Commands.UpdateProject;
using TaskMgr.Api.Application.Projects.Queries.GetProjectById;
using TaskMgr.Api.Application.Projects.Queries.GetProjects;

namespace TaskMgr.Api.Controllers;

/// <summary>
/// Controller for project management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize] // TODO: Enable when authentication is configured
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class ProjectsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<ProjectsController> _logger;

    /// <summary>
    /// Initializes a new instance of the ProjectsController
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="logger">Logger instance</param>
    public ProjectsController(
        ISender sender,
        ILogger<ProjectsController> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all projects for the current user
    /// </summary>
    /// <param name="includeArchived">Whether to include archived projects</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's projects</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<List<ProjectDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDto<List<ProjectDto>>>> GetProjects(
        [FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var projects = await _sender.Send(new GetProjectsQuery { UserId = userId, IncludeArchived = includeArchived }, cancellationToken);

        return Ok(new ApiResponseDto<List<ProjectDto>>
        {
            Data = projects,
            Success = true,
            Message = $"Retrieved {projects.Count} projects"
        });
    }

    /// <summary>
    /// Gets a specific project by ID
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="includeTasks">Whether to include project tasks</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Project details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseDto<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDto<ProjectDto>>> GetProject(
        Guid id,
        [FromQuery] bool includeTasks = false,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var project = await _sender.Send(new GetProjectByIdQuery { ProjectId = id, UserId = userId, IncludeTasks = includeTasks }, cancellationToken);

        if (project == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Project not found",
                Detail = $"Project with ID {id} was not found or you don't have access to it.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(new ApiResponseDto<ProjectDto>
        {
            Data = project,
            Success = true,
            Message = "Project retrieved successfully"
        });
    }

    /// <summary>
    /// Creates a new project
    /// </summary>
    /// <param name="createProjectDto">Project creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created project</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseDto<ProjectDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponseDto<ProjectDto>>> CreateProject(
        [FromBody] CreateProjectDto createProjectDto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new CreateProjectCommand
        {
            Name = createProjectDto.Name,
            Description = createProjectDto.Description,
            Color = createProjectDto.Color,
            DueDate = createProjectDto.DueDate,
            UserId = userId
        };

        var project = await _sender.Send(command, cancellationToken);

        var response = new ApiResponseDto<ProjectDto>
        {
            Data = project,
            Success = true,
            Message = "Project created successfully"
        };

        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, response);
    }

    /// <summary>
    /// Updates an existing project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="updateProjectDto">Project update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated project</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseDto<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDto<ProjectDto>>> UpdateProject(
        Guid id,
        [FromBody] UpdateProjectDto updateProjectDto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new UpdateProjectCommand
        {
            ProjectId = id,
            UserId = userId,
            Name = updateProjectDto.Name,
            Description = updateProjectDto.Description,
            Color = updateProjectDto.Color,
            DueDate = updateProjectDto.DueDate
        };

        var project = await _sender.Send(command, cancellationToken);

        if (project == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Project not found",
                Detail = $"Project with ID {id} was not found or you don't have access to it.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(new ApiResponseDto<ProjectDto>
        {
            Data = project,
            Success = true,
            Message = "Project updated successfully"
        });
    }

    /// <summary>
    /// Deletes a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteProject(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        try
        {
            var deleted = await _sender.Send(new DeleteProjectCommand { ProjectId = id, UserId = userId }, cancellationToken);

            if (!deleted)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Project not found",
                    Detail = $"Project with ID {id} was not found or you don't have access to it.",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }

            return Ok(new ApiResponseDto<object>
            {
                Data = null,
                Success = true,
                Message = "Project deleted successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Cannot delete project",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Archives a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDto<object>>> ArchiveProject(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var archived = await _sender.Send(new ArchiveProjectCommand { ProjectId = id, UserId = userId }, cancellationToken);

        if (!archived)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Project not found",
                Detail = $"Project with ID {id} was not found or you don't have access to it.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(new ApiResponseDto<object>
        {
            Data = null,
            Success = true,
            Message = "Project archived successfully"
        });
    }

    /// <summary>
    /// Unarchives a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("{id:guid}/unarchive")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDto<object>>> UnarchiveProject(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var unarchived = await _sender.Send(new UnarchiveProjectCommand { ProjectId = id, UserId = userId }, cancellationToken);

        if (!unarchived)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Project not found",
                Detail = $"Project with ID {id} was not found or you don't have access to it.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(new ApiResponseDto<object>
        {
            Data = null,
            Success = true,
            Message = "Project unarchived successfully"
        });
    }

    /// <summary>
    /// Gets the current user ID from the JWT token
    /// </summary>
    /// <returns>Current user ID</returns>
    private Guid GetCurrentUserId()
    {
        // TODO: Remove this mock user when authentication is enabled
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value
                         ?? User.FindFirst("oid")?.Value;

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        // Development mode: return Bob Wilson's user ID from seed data
        _logger.LogInformation("No user authentication found, using mock user for development");
        return Guid.Parse("2C6D7627-738C-4D39-9644-E7B703332DC5");
    }
}
