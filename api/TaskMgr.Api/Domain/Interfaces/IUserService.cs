using TaskMgr.Api.Domain.Entities;

namespace TaskMgr.Api.Domain.Interfaces;

/// <summary>
/// Service interface for user-related operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets or creates a user based on Azure AD B2C information
    /// </summary>
    /// <param name="azureObjectId">Azure AD B2C Object ID</param>
    /// <param name="email">User's email</param>
    /// <param name="displayName">User's display name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User entity</returns>
    Task<User> GetOrCreateUserAsync(string azureObjectId, string email, string displayName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a user by their Azure Object ID
    /// </summary>
    /// <param name="azureObjectId">Azure AD B2C Object ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User if found, null otherwise</returns>
    Task<User?> GetUserByAzureObjectIdAsync(string azureObjectId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User if found, null otherwise</returns>
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates user profile information
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="displayName">New display name</param>
    /// <param name="avatar">Profile picture URL</param>
    /// <param name="preferredLanguage">Preferred language</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user</returns>
    Task<User> UpdateUserProfileAsync(Guid userId, string? displayName = null, string? avatar = null, 
        string? preferredLanguage = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Records a user login
    /// </summary>
    /// <param name="azureObjectId">Azure AD B2C Object ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User entity</returns>
    Task<User> RecordUserLoginAsync(string azureObjectId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all active users
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active users</returns>
    Task<List<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deactivates a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
