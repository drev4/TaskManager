using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskMgr.Api.Application.DTOs;

using MediatR;
using TaskMgr.Api.Application.Tasks.Commands.CreateTask;
using TaskMgr.Api.Application.Tasks.Commands.DeleteTask;
using TaskMgr.Api.Application.Tasks.Commands.RecordTaskTime;
using TaskMgr.Api.Application.Tasks.Commands.UpdateTask;
using TaskMgr.Api.Application.Tasks.Queries.GetTaskById;
using TaskMgr.Api.Application.Tasks.Queries.GetTasks;

namespace TaskMgr.Api.Controllers;

/// <summary>
/// Controller for task management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize] // TODO: Enable when authentication is configured
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class TasksController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<TasksController> _logger;

    /// <summary>
    /// Initializes a new instance of the TasksController
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="logger">Logger instance</param>
    public TasksController(
        ISender sender,
        ILogger<TasksController> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets tasks with optional filtering and pagination
    /// </summary>
    /// <param name="projectId">Filter by project ID</param>
    /// <param name="status">Filter by task status</param>
    /// <param name="priority">Filter by task priority</param>
    /// <param name="assignedToUserId">Filter by assigned user ID</param>
    /// <param name="dueBefore">Filter tasks due before this date</param>
    /// <param name="dueAfter">Filter tasks due after this date</param>
    /// <param name="overdueOnly">Include only overdue tasks</param>
    /// <param name="searchTerm">Search term for title or description</param>
    /// <param name="tags">Filter by tags (comma-separated)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="limit">Page size (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of tasks</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDto<TaskItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDto<TaskItemDto>>> GetTasks(
        [FromQuery] Guid? projectId = null,
        [FromQuery] Domain.Enums.TaskStatus? status = null,
        [FromQuery] Domain.Enums.TaskPriority? priority = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] DateTime? dueBefore = null,
        [FromQuery] DateTime? dueAfter = null,
        [FromQuery] bool? overdueOnly = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? tags = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var filter = new TaskFilterDto
        {
            ProjectId = projectId,
            Status = status,
            Priority = priority,
            AssignedToUserId = assignedToUserId,
            DueBefore = dueBefore,
            DueAfter = dueAfter,
            OverdueOnly = overdueOnly,
            SearchTerm = searchTerm,
            Tags = !string.IsNullOrEmpty(tags) ? tags.Split(',').ToList() : null
        };

        // Validate pagination parameters
        if (page < 1) page = 1;
        if (limit < 1) limit = 20;
        if (limit > 100) limit = 100;

        var query = new GetTasksQuery
        {
            UserId = userId,
            Filter = filter,
            PageNumber = page,
            PageSize = limit
        };

        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific task by ID
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseDto<TaskItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDto<TaskItemDto>>> GetTask(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var task = await _sender.Send(new GetTaskByIdQuery { TaskId = id, UserId = userId }, cancellationToken);

        if (task == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Task not found",
                Detail = $"Task with ID {id} was not found or you don't have access to it.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(new ApiResponseDto<TaskItemDto>
        {
            Data = task,
            Success = true,
            Message = "Task retrieved successfully"
        });
    }

    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="createTaskDto">Task creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created task</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseDto<TaskItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponseDto<TaskItemDto>>> CreateTask(
        [FromBody] CreateTaskDto createTaskDto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new CreateTaskCommand
        {
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            ProjectId = createTaskDto.ProjectId,
            Priority = createTaskDto.Priority,
            DueDate = createTaskDto.DueDate,
            EstimatedHours = createTaskDto.EstimatedHours,
            Tags = createTaskDto.Tags ?? new List<string>(),
            AssignedToUserId = createTaskDto.AssignedToUserId,
            UserId = userId
        };

        var task = await _sender.Send(command, cancellationToken);

        var response = new ApiResponseDto<TaskItemDto>
        {
            Data = task,
            Success = true,
            Message = "Task created successfully"
        };

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, response);
    }

    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="updateTaskDto">Task update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated task</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseDto<TaskItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDto<TaskItemDto>>> UpdateTask(
        Guid id,
        [FromBody] UpdateTaskDto updateTaskDto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new UpdateTaskCommand
        {
            TaskId = id,
            UserId = userId,
            Title = updateTaskDto.Title,
            Description = updateTaskDto.Description,
            Status = updateTaskDto.Status,
            Priority = updateTaskDto.Priority,
            AssignedToUserId = updateTaskDto.AssignedToUserId,
            DueDate = updateTaskDto.DueDate,
            EstimatedHours = updateTaskDto.EstimatedHours,
            Tags = updateTaskDto.Tags
        };

        var task = await _sender.Send(command, cancellationToken);

        if (task == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Task not found",
                Detail = $"Task with ID {id} was not found or you don't have access to it.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(new ApiResponseDto<TaskItemDto>
        {
            Data = task,
            Success = true,
            Message = "Task updated successfully"
        });
    }

    /// <summary>
    /// Deletes a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteTask(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var deleted = await _sender.Send(new DeleteTaskCommand { TaskId = id, UserId = userId }, cancellationToken);

        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Task not found",
                Detail = $"Task with ID {id} was not found or you don't have access to it.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(new ApiResponseDto<object>
        {
            Data = null,
            Success = true,
            Message = "Task deleted successfully"
        });
    }

    /// <summary>
    /// Records time spent on a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="recordTimeDto">Time recording data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated task</returns>
    [HttpPost("{id:guid}/time")]
    [ProducesResponseType(typeof(ApiResponseDto<TaskItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDto<TaskItemDto>>> RecordTime(
        Guid id,
        [FromBody] RecordTimeDto recordTimeDto,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new RecordTaskTimeCommand
        {
            TaskId = id,
            UserId = userId,
            Hours = recordTimeDto.Hours,
            Description = recordTimeDto.Description
        };

        var task = await _sender.Send(command, cancellationToken);

        if (task == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Task not found",
                Detail = $"Task with ID {id} was not found or you don't have access to it.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(new ApiResponseDto<TaskItemDto>
        {
            Data = task,
            Success = true,
            Message = "Time recorded successfully"
        });
    }

    /// <summary>
    /// Gets the current user ID from the JWT token
    /// </summary>
    /// <returns>Current user ID</returns>
    private Guid GetCurrentUserId()
    {
        // TODO: Remove this mock user when authentication is enabled
        // For development without authentication, return the first user ID from database
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value
                         ?? User.FindFirst("oid")?.Value;

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        // Development mode: return Bob Wilson's user ID from seed data
        // This allows testing without authentication
        _logger.LogInformation("No user authentication found, using mock user for development");

        // Bob Wilson's ID from seed data
        return Guid.Parse("2C6D7627-738C-4D39-9644-E7B703332DC5");
    }
}
