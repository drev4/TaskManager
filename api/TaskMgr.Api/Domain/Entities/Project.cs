namespace TaskMgr.Api.Domain.Entities;

/// <summary>
/// Represents a project in the system
/// </summary>
public class Project : BaseEntity
{
    /// <summary>
    /// Project name
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Project description
    /// </summary>
    public string? Description { get; private set; }
    
    /// <summary>
    /// ID of the user who owns this project
    /// </summary>
    public Guid OwnerUserId { get; private set; }
    
    /// <summary>
    /// Project color for UI customization
    /// </summary>
    public string? Color { get; private set; }
    
    /// <summary>
    /// Whether the project is archived
    /// </summary>
    public bool IsArchived { get; private set; } = false;
    
    /// <summary>
    /// Project deadline (optional)
    /// </summary>
    public DateTime? DueDate { get; private set; }
    
    /// <summary>
    /// Date when the project was completed
    /// </summary>
    public DateTime? CompletedAt { get; private set; }
    
    // Navigation properties
    /// <summary>
    /// User who owns this project
    /// </summary>
    public virtual User Owner { get; private set; } = null!;
    
    /// <summary>
    /// Tasks belonging to this project
    /// </summary>
    public virtual ICollection<TaskItem> Tasks { get; private set; } = new List<TaskItem>();
    
    // Private constructor for EF Core
    private Project() { }
    
    /// <summary>
    /// Creates a new project instance
    /// </summary>
    /// <param name="name">Project name</param>
    /// <param name="ownerUserId">ID of the project owner</param>
    /// <param name="description">Project description</param>
    public Project(string name, Guid ownerUserId, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be null or empty", nameof(name));
        
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("Owner user ID cannot be empty", nameof(ownerUserId));
        
        Name = name.Trim();
        OwnerUserId = ownerUserId;
        Description = description?.Trim();
    }
    
    /// <summary>
    /// Updates project information
    /// </summary>
    /// <param name="name">New project name</param>
    /// <param name="description">New project description</param>
    /// <param name="color">New project color</param>
    /// <param name="dueDate">New project due date</param>
    public void Update(string? name = null, string? description = null, string? color = null, DateTime? dueDate = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name.Trim();
        
        Description = description?.Trim();
        Color = color;
        DueDate = dueDate;
        
        SetUpdated();
    }
    
    /// <summary>
    /// Archives the project
    /// </summary>
    public void Archive()
    {
        IsArchived = true;
        SetUpdated();
    }
    
    /// <summary>
    /// Unarchives the project
    /// </summary>
    public void Unarchive()
    {
        IsArchived = false;
        SetUpdated();
    }
    
    /// <summary>
    /// Marks the project as completed
    /// </summary>
    public void MarkAsCompleted()
    {
        CompletedAt = DateTime.UtcNow;
        SetUpdated();
    }
    
    /// <summary>
    /// Marks the project as incomplete
    /// </summary>
    public void MarkAsIncomplete()
    {
        CompletedAt = null;
        SetUpdated();
    }
    
    /// <summary>
    /// Gets the project completion percentage based on completed tasks
    /// </summary>
    /// <returns>Completion percentage (0-100)</returns>
    public int GetCompletionPercentage()
    {
        if (!Tasks.Any())
            return 0;
        
        var completedTasks = Tasks.Count(t => t.Status == Enums.TaskStatus.Done);
        return (int)Math.Round((double)completedTasks / Tasks.Count * 100);
    }
    
    /// <summary>
    /// Gets the count of tasks by status
    /// </summary>
    /// <param name="status">Task status to count</param>
    /// <returns>Number of tasks with the specified status</returns>
    public int GetTaskCountByStatus(Enums.TaskStatus status)
    {
        return Tasks.Count(t => t.Status == status);
    }
}
