using TaskMgr.Api.Domain.Enums;

namespace TaskMgr.Api.Domain.Entities;

/// <summary>
/// Represents a task in the system
/// </summary>
public class TaskItem : BaseEntity
{
    /// <summary>
    /// Task title
    /// </summary>
    public string Title { get; private set; } = string.Empty;
    
    /// <summary>
    /// Task description
    /// </summary>
    public string? Description { get; private set; }
    
    /// <summary>
    /// Current status of the task
    /// </summary>
    public Domain.Enums.TaskStatus Status { get; private set; } = Domain.Enums.TaskStatus.Todo;
    
    /// <summary>
    /// Priority level of the task
    /// </summary>
    public TaskPriority Priority { get; private set; } = TaskPriority.Medium;
    
    /// <summary>
    /// ID of the project this task belongs to
    /// </summary>
    public Guid ProjectId { get; private set; }
    
    /// <summary>
    /// ID of the user assigned to this task (optional)
    /// </summary>
    public Guid? AssignedToUserId { get; private set; }
    
    /// <summary>
    /// Task due date (optional)
    /// </summary>
    public DateTime? DueDate { get; private set; }
    
    /// <summary>
    /// Date when the task was completed
    /// </summary>
    public DateTime? CompletedAt { get; private set; }
    
    /// <summary>
    /// Estimated time to complete the task in hours
    /// </summary>
    public decimal? EstimatedHours { get; private set; }
    
    /// <summary>
    /// Actual time spent on the task in hours
    /// </summary>
    public decimal? ActualHours { get; private set; }
    
    /// <summary>
    /// Tags associated with the task
    /// </summary>
    public string? Tags { get; private set; }
    
    // Navigation properties
    /// <summary>
    /// Project this task belongs to
    /// </summary>
    public virtual Project Project { get; private set; } = null!;
    
    /// <summary>
    /// User assigned to this task
    /// </summary>
    public virtual User? AssignedTo { get; private set; }
    
    // Private constructor for EF Core
    private TaskItem() { }
    
    /// <summary>
    /// Creates a new task instance
    /// </summary>
    /// <param name="title">Task title</param>
    /// <param name="projectId">ID of the project this task belongs to</param>
    /// <param name="priority">Task priority level</param>
    /// <param name="description">Task description</param>
    public TaskItem(string title, Guid projectId, TaskPriority priority = TaskPriority.Medium, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be null or empty", nameof(title));
        
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project ID cannot be empty", nameof(projectId));
        
        Title = title.Trim();
        ProjectId = projectId;
        Priority = priority;
        Description = description?.Trim();
    }
    
    /// <summary>
    /// Updates task information
    /// </summary>
    /// <param name="title">New task title</param>
    /// <param name="description">New task description</param>
    /// <param name="priority">New task priority</param>
    /// <param name="dueDate">New task due date</param>
    /// <param name="estimatedHours">New estimated hours</param>
    /// <param name="tags">New tags</param>
    public void Update(string? title = null, string? description = null, TaskPriority? priority = null, 
        DateTime? dueDate = null, decimal? estimatedHours = null, string? tags = null)
    {
        if (!string.IsNullOrWhiteSpace(title))
            Title = title.Trim();
        
        Description = description?.Trim();
        
        if (priority.HasValue)
            Priority = priority.Value;
        
        DueDate = dueDate;
        EstimatedHours = estimatedHours;
        Tags = tags?.Trim();
        
        SetUpdated();
    }
    
    /// <summary>
    /// Updates the task status
    /// </summary>
    /// <param name="newStatus">New task status</param>
    public void UpdateStatus(Domain.Enums.TaskStatus newStatus)
    {
        var previousStatus = Status;
        Status = newStatus;
        
        // Handle completion logic
        if (newStatus == Domain.Enums.TaskStatus.Done && previousStatus != Domain.Enums.TaskStatus.Done)
        {
            CompletedAt = DateTime.UtcNow;
        }
        else if (newStatus != Domain.Enums.TaskStatus.Done && previousStatus == Domain.Enums.TaskStatus.Done)
        {
            CompletedAt = null;
        }
        
        SetUpdated();
    }
    
    /// <summary>
    /// Assigns the task to a user
    /// </summary>
    /// <param name="userId">ID of the user to assign the task to</param>
    public void AssignTo(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        
        AssignedToUserId = userId;
        SetUpdated();
    }
    
    /// <summary>
    /// Unassigns the task from any user
    /// </summary>
    public void Unassign()
    {
        AssignedToUserId = null;
        SetUpdated();
    }
    
    /// <summary>
    /// Records actual time spent on the task
    /// </summary>
    /// <param name="hours">Hours spent on the task</param>
    public void RecordTimeSpent(decimal hours)
    {
        if (hours < 0)
            throw new ArgumentException("Hours cannot be negative", nameof(hours));
        
        ActualHours = (ActualHours ?? 0) + hours;
        SetUpdated();
    }
    
    /// <summary>
    /// Checks if the task is overdue
    /// </summary>
    /// <returns>True if the task is overdue</returns>
    public bool IsOverdue()
    {
        return DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != Domain.Enums.TaskStatus.Done;
    }
    
    /// <summary>
    /// Checks if the task is due soon (within the next 24 hours)
    /// </summary>
    /// <returns>True if the task is due soon</returns>
    public bool IsDueSoon()
    {
        return DueDate.HasValue && DueDate.Value <= DateTime.UtcNow.AddHours(24) && Status != Domain.Enums.TaskStatus.Done;
    }
    
    /// <summary>
    /// Gets the task tags as a list
    /// </summary>
    /// <returns>List of tags</returns>
    public List<string> GetTagsList()
    {
        if (string.IsNullOrWhiteSpace(Tags))
            return new List<string>();
        
        return Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(tag => tag.Trim())
                  .Where(tag => !string.IsNullOrEmpty(tag))
                  .ToList();
    }
    
    /// <summary>
    /// Sets the task tags from a list
    /// </summary>
    /// <param name="tags">List of tags</param>
    public void SetTags(IEnumerable<string> tags)
    {
        Tags = string.Join(",", tags.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim()));
        SetUpdated();
    }
}
