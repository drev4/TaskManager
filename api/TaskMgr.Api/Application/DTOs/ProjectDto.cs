namespace TaskMgr.Api.Application.DTOs;

/// <summary>
/// Data Transfer Object for Project information
/// </summary>
public class ProjectDto
{
    /// <summary>
    /// Project unique identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Project name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Project description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Project owner user ID
    /// </summary>
    public Guid OwnerUserId { get; set; }
    
    /// <summary>
    /// Project owner information
    /// </summary>
    public UserDto? Owner { get; set; }
    
    /// <summary>
    /// Project color for UI customization
    /// </summary>
    public string? Color { get; set; }
    
    /// <summary>
    /// Whether the project is archived
    /// </summary>
    public bool IsArchived { get; set; }
    
    /// <summary>
    /// Project deadline
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Date when the project was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Date when the project was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Date when the project was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Total number of tasks in the project
    /// </summary>
    public int TotalTasksCount { get; set; }
    
    /// <summary>
    /// Number of completed tasks in the project
    /// </summary>
    public int CompletedTasksCount { get; set; }
    
    /// <summary>
    /// Project completion percentage
    /// </summary>
    public int CompletionPercentage { get; set; }
    
    /// <summary>
    /// Project tasks (optional, for detailed views)
    /// </summary>
    public List<TaskItemDto>? Tasks { get; set; }
}

/// <summary>
/// DTO for creating a new project
/// </summary>
public class CreateProjectDto
{
    /// <summary>
    /// Project name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Project description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Project color for UI customization
    /// </summary>
    public string? Color { get; set; }
    
    /// <summary>
    /// Project deadline
    /// </summary>
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// DTO for updating an existing project
/// </summary>
public class UpdateProjectDto
{
    /// <summary>
    /// New project name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// New project description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// New project color
    /// </summary>
    public string? Color { get; set; }
    
    /// <summary>
    /// New project deadline
    /// </summary>
    public DateTime? DueDate { get; set; }
}
