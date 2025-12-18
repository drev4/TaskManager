using AutoMapper;
using Microsoft.Extensions.Logging;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Domain.Enums;
using TaskMgr.Api.Domain.Interfaces;
using System.Linq.Expressions;

namespace TaskMgr.Api.Application.Services;

/// <summary>
/// Service for task-related operations
/// </summary>
public class TaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TaskService> _logger;

    /// <summary>
    /// Initializes a new instance of the TaskService
    /// </summary>
    /// <param name="unitOfWork">Unit of work</param>
    /// <param name="mapper">AutoMapper instance</param>
    /// <param name="logger">Logger instance</param>
    public TaskService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TaskService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets tasks with optional filtering and pagination
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="filter">Task filter options</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated task results</returns>
    public async Task<PagedResponseDto<TaskItemDto>> GetTasksAsync(
        Guid userId, 
        TaskFilterDto? filter = null, 
        int pageNumber = 1, 
        int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting tasks for user {UserId} with filter", userId);

        // Build the filter expression
        Expression<Func<TaskItem, bool>> predicate = BuildTaskFilter(userId, filter);

        // Get paginated results
        var (tasks, totalCount) = await _unitOfWork.Tasks.GetPagedAsync(
            pageNumber, 
            pageSize, 
            predicate, 
            t => t.CreatedAt, 
            cancellationToken);

        var taskDtos = _mapper.Map<List<TaskItemDto>>(tasks);

        var pagination = new PaginationDto
        {
            Page = pageNumber,
            Limit = pageSize,
            Total = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            HasPrevious = pageNumber > 1,
            HasNext = pageNumber * pageSize < totalCount
        };

        _logger.LogDebug("Found {TaskCount} tasks for user {UserId}", taskDtos.Count, userId);

        return new PagedResponseDto<TaskItemDto>
        {
            Data = taskDtos,
            Pagination = pagination,
            Success = true
        };
    }

    /// <summary>
    /// Gets a task by ID
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="userId">User ID (for ownership verification)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task DTO or null if not found</returns>
    public async Task<TaskItemDto?> GetTaskByIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting task {TaskId} for user {UserId}", taskId, userId);

        var task = await _unitOfWork.Tasks.GetSingleAsync(
            t => t.Id == taskId && 
                 (t.Project.OwnerUserId == userId || t.AssignedToUserId == userId),
            cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found or not accessible by user {UserId}", taskId, userId);
            return null;
        }

        return _mapper.Map<TaskItemDto>(task);
    }

    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="createTaskDto">Task creation data</param>
    /// <param name="userId">Creator user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created task DTO</returns>
    public async Task<TaskItemDto> CreateTaskAsync(CreateTaskDto createTaskDto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new task '{TaskTitle}' for user {UserId}", createTaskDto.Title, userId);

        // Verify project exists and user owns it
        var project = await _unitOfWork.Projects.GetSingleAsync(
            p => p.Id == createTaskDto.ProjectId && p.OwnerUserId == userId,
            cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project {ProjectId} not found or not owned by user {UserId}", createTaskDto.ProjectId, userId);
            throw new InvalidOperationException($"Project with ID {createTaskDto.ProjectId} not found or not accessible");
        }

        // Verify assigned user exists if specified
        if (createTaskDto.AssignedToUserId.HasValue)
        {
            var assignedUser = await _unitOfWork.Users.GetByIdAsync(createTaskDto.AssignedToUserId.Value, cancellationToken);
            if (assignedUser == null || !assignedUser.IsActive)
            {
                _logger.LogWarning("Assigned user {AssignedUserId} not found or inactive", createTaskDto.AssignedToUserId);
                throw new InvalidOperationException($"Assigned user with ID {createTaskDto.AssignedToUserId} not found or inactive");
            }
        }

        var task = new TaskItem(createTaskDto.Title, createTaskDto.ProjectId, createTaskDto.Priority, createTaskDto.Description);
        
        if (createTaskDto.AssignedToUserId.HasValue)
            task.AssignTo(createTaskDto.AssignedToUserId.Value);
        
        if (createTaskDto.DueDate.HasValue)
            task.Update(dueDate: createTaskDto.DueDate);
        
        if (createTaskDto.EstimatedHours.HasValue)
            task.Update(estimatedHours: createTaskDto.EstimatedHours);
        
        if (createTaskDto.Tags.Any())
            task.SetTags(createTaskDto.Tags);

        await _unitOfWork.Tasks.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created task {TaskId} for user {UserId}", task.Id, userId);
        return _mapper.Map<TaskItemDto>(task);
    }

    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="updateTaskDto">Task update data</param>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated task DTO</returns>
    public async Task<TaskItemDto?> UpdateTaskAsync(Guid taskId, UpdateTaskDto updateTaskDto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating task {TaskId} for user {UserId}", taskId, userId);

        var task = await _unitOfWork.Tasks.GetSingleAsync(
            t => t.Id == taskId && 
                 (t.Project.OwnerUserId == userId || t.AssignedToUserId == userId),
            cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found or not accessible by user {UserId}", taskId, userId);
            return null;
        }

        // Update basic properties
        task.Update(
            updateTaskDto.Title,
            updateTaskDto.Description,
            updateTaskDto.Priority,
            updateTaskDto.DueDate,
            updateTaskDto.EstimatedHours);

        // Update status if provided
        if (updateTaskDto.Status.HasValue)
            task.UpdateStatus(updateTaskDto.Status.Value);

        // Update assignment
        if (updateTaskDto.AssignedToUserId.HasValue)
        {
            if (updateTaskDto.AssignedToUserId == Guid.Empty)
            {
                task.Unassign();
            }
            else
            {
                var assignedUser = await _unitOfWork.Users.GetByIdAsync(updateTaskDto.AssignedToUserId.Value, cancellationToken);
                if (assignedUser == null || !assignedUser.IsActive)
                {
                    _logger.LogWarning("Assigned user {AssignedUserId} not found or inactive", updateTaskDto.AssignedToUserId);
                    throw new InvalidOperationException($"Assigned user with ID {updateTaskDto.AssignedToUserId} not found or inactive");
                }
                
                task.AssignTo(updateTaskDto.AssignedToUserId.Value);
            }
        }

        // Update tags
        if (updateTaskDto.Tags != null)
            task.SetTags(updateTaskDto.Tags);

        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated task {TaskId}", taskId);
        return _mapper.Map<TaskItemDto>(task);
    }

    /// <summary>
    /// Deletes a task
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteTaskAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting task {TaskId} for user {UserId}", taskId, userId);

        var task = await _unitOfWork.Tasks.GetSingleAsync(
            t => t.Id == taskId && t.Project.OwnerUserId == userId,
            cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found or not owned by user {UserId}", taskId, userId);
            return false;
        }

        _unitOfWork.Tasks.Remove(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted task {TaskId}", taskId);
        return true;
    }

    /// <summary>
    /// Records time spent on a task
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="recordTimeDto">Time recording data</param>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated task DTO</returns>
    public async Task<TaskItemDto?> RecordTimeAsync(Guid taskId, RecordTimeDto recordTimeDto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Recording {Hours} hours for task {TaskId} by user {UserId}", recordTimeDto.Hours, taskId, userId);

        var task = await _unitOfWork.Tasks.GetSingleAsync(
            t => t.Id == taskId && 
                 (t.Project.OwnerUserId == userId || t.AssignedToUserId == userId),
            cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found or not accessible by user {UserId}", taskId, userId);
            return null;
        }

        task.RecordTimeSpent(recordTimeDto.Hours);
        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully recorded time for task {TaskId}", taskId);
        return _mapper.Map<TaskItemDto>(task);
    }

    /// <summary>
    /// Gets dashboard statistics for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard statistics</returns>
    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting dashboard stats for user {UserId}", userId);

        var projects = await _unitOfWork.Projects.FindAsync(p => p.OwnerUserId == userId && !p.IsArchived, cancellationToken);
        var projectIds = projects.Select(p => p.Id).ToList();
        
        var allTasks = await _unitOfWork.Tasks.FindAsync(t => projectIds.Contains(t.ProjectId), cancellationToken);

        var now = DateTime.UtcNow;
        var weekStart = now.AddDays(-(int)now.DayOfWeek);

        var stats = new DashboardStatsDto
        {
            TotalProjects = projects.Count,
            TotalTasks = allTasks.Count,
            CompletedTasks = allTasks.Count(t => t.Status == Domain.Enums.TaskStatus.Done),
            PendingTasks = allTasks.Count(t => t.Status != Domain.Enums.TaskStatus.Done),
            OverdueTasks = allTasks.Count(t => t.IsOverdue()),
            TasksCreatedThisWeek = allTasks.Count(t => t.CreatedAt >= weekStart),
            TasksCompletedThisWeek = allTasks.Count(t => t.CompletedAt >= weekStart)
        };

        stats.CompletionPercentage = stats.TotalTasks > 0 
            ? Math.Round((decimal)stats.CompletedTasks / stats.TotalTasks * 100, 1)
            : 0;

        _logger.LogDebug("Dashboard stats calculated for user {UserId}: {TotalTasks} tasks, {CompletionPercentage}% complete", 
            userId, stats.TotalTasks, stats.CompletionPercentage);

        return stats;
    }

    /// <summary>
    /// Builds a filter expression for tasks
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="filter">Filter options</param>
    /// <returns>Filter expression</returns>
    private static Expression<Func<TaskItem, bool>> BuildTaskFilter(Guid userId, TaskFilterDto? filter)
    {
        Expression<Func<TaskItem, bool>> predicate = t => 
            t.Project.OwnerUserId == userId || t.AssignedToUserId == userId;

        if (filter == null) return predicate;

        if (filter.ProjectId.HasValue)
        {
            var projectFilter = predicate;
            predicate = t => projectFilter.Compile()(t) && t.ProjectId == filter.ProjectId.Value;
        }

        if (filter.Status.HasValue)
        {
            var statusFilter = predicate;
            predicate = t => statusFilter.Compile()(t) && t.Status == filter.Status.Value;
        }

        if (filter.Priority.HasValue)
        {
            var priorityFilter = predicate;
            predicate = t => priorityFilter.Compile()(t) && t.Priority == filter.Priority.Value;
        }

        if (filter.AssignedToUserId.HasValue)
        {
            var assignedFilter = predicate;
            predicate = t => assignedFilter.Compile()(t) && t.AssignedToUserId == filter.AssignedToUserId.Value;
        }

        if (filter.DueBefore.HasValue)
        {
            var dueBeforeFilter = predicate;
            predicate = t => dueBeforeFilter.Compile()(t) && t.DueDate <= filter.DueBefore.Value;
        }

        if (filter.DueAfter.HasValue)
        {
            var dueAfterFilter = predicate;
            predicate = t => dueAfterFilter.Compile()(t) && t.DueDate >= filter.DueAfter.Value;
        }

        if (filter.OverdueOnly.HasValue && filter.OverdueOnly.Value)
        {
            var overdueFilter = predicate;
            predicate = t => overdueFilter.Compile()(t) && 
                           t.DueDate.HasValue && 
                           t.DueDate.Value < DateTime.UtcNow && 
                           t.Status != Domain.Enums.TaskStatus.Done;
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchFilter = predicate;
            var searchTerm = filter.SearchTerm.ToLower();
            predicate = t => searchFilter.Compile()(t) && 
                           (t.Title.ToLower().Contains(searchTerm) || 
                            (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
        }

        return predicate;
    }
}
