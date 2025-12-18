namespace TaskMgr.Api.Domain.Enums;

/// <summary>
/// Represents the status of a task
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task is pending and hasn't been started
    /// </summary>
    Todo = 0,
    
    /// <summary>
    /// Task is currently being worked on
    /// </summary>
    InProgress = 1,
    
    /// <summary>
    /// Task has been completed
    /// </summary>
    Done = 2
}
