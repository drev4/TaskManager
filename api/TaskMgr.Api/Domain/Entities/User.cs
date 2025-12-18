namespace TaskMgr.Api.Domain.Entities;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// User's email address (from Azure AD B2C)
    /// </summary>
    public string Email { get; private set; } = string.Empty;
    
    /// <summary>
    /// User's display name (from Azure AD B2C)
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;
    
    /// <summary>
    /// Azure AD B2C Object ID
    /// </summary>
    public string AzureObjectId { get; private set; } = string.Empty;
    
    /// <summary>
    /// User's profile picture URL
    /// </summary>
    public string? Avatar { get; private set; }
    
    /// <summary>
    /// User's preferred language/locale
    /// </summary>
    public string? PreferredLanguage { get; private set; }
    
    /// <summary>
    /// Whether the user is active in the system
    /// </summary>
    public bool IsActive { get; private set; } = true;
    
    /// <summary>
    /// Date when the user last accessed the system
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }
    
    // Navigation properties
    /// <summary>
    /// Projects owned by this user
    /// </summary>
    public virtual ICollection<Project> OwnedProjects { get; private set; } = new List<Project>();
    
    /// <summary>
    /// Tasks assigned to this user
    /// </summary>
    public virtual ICollection<TaskItem> AssignedTasks { get; private set; } = new List<TaskItem>();
    
    // Private constructor for EF Core
    private User() { }
    
    /// <summary>
    /// Creates a new user instance
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="displayName">User's display name</param>
    /// <param name="azureObjectId">Azure AD B2C Object ID</param>
    public User(string email, string displayName, string azureObjectId)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be null or empty", nameof(displayName));
        
        if (string.IsNullOrWhiteSpace(azureObjectId))
            throw new ArgumentException("Azure Object ID cannot be null or empty", nameof(azureObjectId));
        
        Email = email.ToLowerInvariant();
        DisplayName = displayName;
        AzureObjectId = azureObjectId;
    }
    
    /// <summary>
    /// Updates user profile information
    /// </summary>
    /// <param name="displayName">New display name</param>
    /// <param name="avatar">Profile picture URL</param>
    /// <param name="preferredLanguage">Preferred language</param>
    public void UpdateProfile(string? displayName = null, string? avatar = null, string? preferredLanguage = null)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
            DisplayName = displayName;
        
        Avatar = avatar;
        PreferredLanguage = preferredLanguage;
        
        SetUpdated();
    }
    
    /// <summary>
    /// Records user login
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        SetUpdated();
    }
    
    /// <summary>
    /// Deactivates the user
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }
    
    /// <summary>
    /// Reactivates the user
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdated();
    }
}
