using TaskMgr.Api.Domain.Enums;

namespace TaskMgr.Api.Application.DTOs;

/// <summary>
/// Data Transfer Object for Task information
/// </summary>
public class TaskItemDto
{
    /// <summary>
    /// Task unique identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Task title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Task description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Current status of the task
    /// </summary>
    public Domain.Enums.TaskStatus Status { get; set; }
    
    /// <summary>
    /// Priority level of the task
    /// </summary>
    public TaskPriority Priority { get; set; }
    
    /// <summary>
    /// Project ID this task belongs to
    /// </summary>
    public Guid ProjectId { get; set; }
    
    /// <summary>
    /// Project information (optional)
    /// </summary>
    public ProjectDto? Project { get; set; }
    
    /// <summary>
    /// User ID assigned to this task
    /// </summary>
    public Guid? AssignedToUserId { get; set; }
    
    /// <summary>
    /// User assigned to this task (optional)
    /// </summary>
    public UserDto? AssignedTo { get; set; }
    
    /// <summary>
    /// Task due date
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Date when the task was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Estimated time to complete in hours
    /// </summary>
    public decimal? EstimatedHours { get; set; }
    
    /// <summary>
    /// Actual time spent in hours
    /// </summary>
    public decimal? ActualHours { get; set; }
    
    /// <summary>
    /// Task tags
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Date when the task was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Date when the task was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Whether the task is overdue
    /// </summary>
    public bool IsOverdue { get; set; }
    
    /// <summary>
    /// Whether the task is due soon
    /// </summary>
    public bool IsDueSoon { get; set; }
}

/// <summary>
/// DTO for creating a new task
/// </summary>
public class CreateTaskDto
{
    /// <summary>
    /// Task title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Task description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Priority level of the task
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    
    /// <summary>
    /// Project ID this task belongs to
    /// </summary>
    public Guid ProjectId { get; set; }
    
    /// <summary>
    /// User ID to assign this task to
    /// </summary>
    public Guid? AssignedToUserId { get; set; }
    
    /// <summary>
    /// Task due date
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Estimated time to complete in hours
    /// </summary>
    public decimal? EstimatedHours { get; set; }
    
    /// <summary>
    /// Task tags
    /// </summary>
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// DTO for updating an existing task
/// </summary>
public class UpdateTaskDto
{
    /// <summary>
    /// New task title
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// New task description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// New task status
    /// </summary>
    public Domain.Enums.TaskStatus? Status { get; set; }
    
    /// <summary>
    /// New priority level
    /// </summary>
    public TaskPriority? Priority { get; set; }
    
    /// <summary>
    /// New user assignment
    /// </summary>
    public Guid? AssignedToUserId { get; set; }
    
    /// <summary>
    /// New due date
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// New estimated hours
    /// </summary>
    public decimal? EstimatedHours { get; set; }
    
    /// <summary>
    /// New task tags
    /// </summary>
    public List<string>? Tags { get; set; }
}

/// <summary>
/// DTO for recording time spent on a task
/// </summary>
public class RecordTimeDto
{
    /// <summary>
    /// Hours spent on the task
    /// </summary>
    public decimal Hours { get; set; }
    
    /// <summary>
    /// Description of the work done
    /// </summary>
    public string? Description { get; set; }
}
