namespace TaskMgr.Api.Application.DTOs;

/// <summary>
/// Data Transfer Object for User information
/// </summary>
public class UserDto
{
    /// <summary>
    /// User unique identifier
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User's display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's profile picture URL
    /// </summary>
    public string? Avatar { get; set; }
    
    /// <summary>
    /// User's preferred language
    /// </summary>
    public string? PreferredLanguage { get; set; }
    
    /// <summary>
    /// Whether the user is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Date when the user was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Date when the user last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// DTO for updating user profile
/// </summary>
public class UpdateUserProfileDto
{
    /// <summary>
    /// New display name
    /// </summary>
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// New profile picture URL
    /// </summary>
    public string? Avatar { get; set; }
    
    /// <summary>
    /// New preferred language
    /// </summary>
    public string? PreferredLanguage { get; set; }
}
