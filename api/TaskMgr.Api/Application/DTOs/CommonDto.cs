namespace TaskMgr.Api.Application.DTOs;

/// <summary>
/// Generic paginated response DTO
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public class PagedResponseDto<T>
{
    /// <summary>
    /// List of items for the current page
    /// </summary>
    public List<T> Data { get; set; } = new();
    
    /// <summary>
    /// Pagination information
    /// </summary>
    public PaginationDto Pagination { get; set; } = new();
    
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// Message associated with the response
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Pagination information DTO
/// </summary>
public class PaginationDto
{
    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Number of items per page
    /// </summary>
    public int Limit { get; set; }
    
    /// <summary>
    /// Total number of items
    /// </summary>
    public int Total { get; set; }
    
    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPrevious { get; set; }
    
    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNext { get; set; }
}

/// <summary>
/// Standard API response DTO
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public class ApiResponseDto<T>
{
    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// Message associated with the response
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// List of errors (if any)
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Dashboard statistics DTO
/// </summary>
public class DashboardStatsDto
{
    /// <summary>
    /// Total number of projects
    /// </summary>
    public int TotalProjects { get; set; }
    
    /// <summary>
    /// Total number of tasks
    /// </summary>
    public int TotalTasks { get; set; }
    
    /// <summary>
    /// Number of completed tasks
    /// </summary>
    public int CompletedTasks { get; set; }
    
    /// <summary>
    /// Number of pending tasks
    /// </summary>
    public int PendingTasks { get; set; }
    
    /// <summary>
    /// Number of overdue tasks
    /// </summary>
    public int OverdueTasks { get; set; }
    
    /// <summary>
    /// Overall completion percentage
    /// </summary>
    public decimal CompletionPercentage { get; set; }
    
    /// <summary>
    /// Tasks created this week
    /// </summary>
    public int TasksCreatedThisWeek { get; set; }
    
    /// <summary>
    /// Tasks completed this week
    /// </summary>
    public int TasksCompletedThisWeek { get; set; }
}

/// <summary>
/// Recent activity DTO
/// </summary>
public class RecentActivityDto
{
    /// <summary>
    /// Activity unique identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Type of activity
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Activity description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Date and time when the activity occurred
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// User who performed the activity
    /// </summary>
    public UserDto User { get; set; } = new();
    
    /// <summary>
    /// Related item information
    /// </summary>
    public RelatedItemDto? RelatedItem { get; set; }
}

/// <summary>
/// Related item information for activities
/// </summary>
public class RelatedItemDto
{
    /// <summary>
    /// Item unique identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Item name or title
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of item (task, project, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Task filter options DTO
/// </summary>
public class TaskFilterDto
{
    /// <summary>
    /// Filter by project ID
    /// </summary>
    public Guid? ProjectId { get; set; }
    
    /// <summary>
    /// Filter by task status
    /// </summary>
    public Domain.Enums.TaskStatus? Status { get; set; }
    
    /// <summary>
    /// Filter by task priority
    /// </summary>
    public Domain.Enums.TaskPriority? Priority { get; set; }
    
    /// <summary>
    /// Filter by assigned user ID
    /// </summary>
    public Guid? AssignedToUserId { get; set; }
    
    /// <summary>
    /// Filter by due date (tasks due before this date)
    /// </summary>
    public DateTime? DueBefore { get; set; }
    
    /// <summary>
    /// Filter by due date (tasks due after this date)
    /// </summary>
    public DateTime? DueAfter { get; set; }
    
    /// <summary>
    /// Include overdue tasks only
    /// </summary>
    public bool? OverdueOnly { get; set; }
    
    /// <summary>
    /// Search term for title or description
    /// </summary>
    public string? SearchTerm { get; set; }
    
    /// <summary>
    /// Filter by tags
    /// </summary>
    public List<string>? Tags { get; set; }
}
