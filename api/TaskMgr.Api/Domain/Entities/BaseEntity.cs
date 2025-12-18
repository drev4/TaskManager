namespace TaskMgr.Api.Domain.Entities;

/// <summary>
/// Base entity with common properties for all domain entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();
    
    /// <summary>
    /// Date and time when the entity was created
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date and time when the entity was last updated
    /// </summary>
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User ID who created the entity
    /// </summary>
    public string? CreatedBy { get; protected set; }
    
    /// <summary>
    /// User ID who last updated the entity
    /// </summary>
    public string? UpdatedBy { get; protected set; }
    
    /// <summary>
    /// Updates the entity's modification metadata
    /// </summary>
    /// <param name="updatedBy">User ID who is updating the entity</param>
    public virtual void SetUpdated(string? updatedBy = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
    
    /// <summary>
    /// Sets the creation metadata for the entity
    /// </summary>
    /// <param name="createdBy">User ID who created the entity</param>
    public virtual void SetCreated(string? createdBy = null)
    {
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = createdBy;
    }
}
