using AutoMapper;
using Microsoft.Extensions.Logging;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Application.Services;

/// <summary>
/// Service for user-related operations
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// Initializes a new instance of the UserService
    /// </summary>
    /// <param name="unitOfWork">Unit of work</param>
    /// <param name="mapper">AutoMapper instance</param>
    /// <param name="logger">Logger instance</param>
    public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<User> GetOrCreateUserAsync(string azureObjectId, string email, string displayName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting or creating user for Azure Object ID: {AzureObjectId}", azureObjectId);

        var existingUser = await _unitOfWork.Users.GetSingleAsync(
            u => u.AzureObjectId == azureObjectId, 
            cancellationToken);

        if (existingUser != null)
        {
            _logger.LogDebug("User found for Azure Object ID: {AzureObjectId}", azureObjectId);
            
            // Update user info in case it changed in Azure AD
            existingUser.UpdateProfile(displayName);
            _unitOfWork.Users.Update(existingUser);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return existingUser;
        }

        _logger.LogInformation("Creating new user for Azure Object ID: {AzureObjectId}", azureObjectId);

        var newUser = new User(email, displayName, azureObjectId);
        await _unitOfWork.Users.AddAsync(newUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created user with ID: {UserId}", newUser.Id);
        return newUser;
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByAzureObjectIdAsync(string azureObjectId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by Azure Object ID: {AzureObjectId}", azureObjectId);

        return await _unitOfWork.Users.GetSingleAsync(
            u => u.AzureObjectId == azureObjectId && u.IsActive, 
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by ID: {UserId}", userId);

        return await _unitOfWork.Users.GetSingleAsync(
            u => u.Id == userId && u.IsActive, 
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User> UpdateUserProfileAsync(Guid userId, string? displayName = null, string? avatar = null, 
        string? preferredLanguage = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating user profile for user ID: {UserId}", userId);

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found with ID: {UserId}", userId);
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Attempted to update inactive user: {UserId}", userId);
            throw new InvalidOperationException("Cannot update inactive user");
        }

        user.UpdateProfile(displayName, avatar, preferredLanguage);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated user profile for user ID: {UserId}", userId);
        return user;
    }

    /// <inheritdoc />
    public async Task<User> RecordUserLoginAsync(string azureObjectId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Recording login for Azure Object ID: {AzureObjectId}", azureObjectId);

        var user = await _unitOfWork.Users.GetSingleAsync(
            u => u.AzureObjectId == azureObjectId, 
            cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found for login record: {AzureObjectId}", azureObjectId);
            throw new InvalidOperationException($"User with Azure Object ID {azureObjectId} not found");
        }

        user.RecordLogin();
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully recorded login for user ID: {UserId}", user.Id);
        return user;
    }

    /// <inheritdoc />
    public async Task<List<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all active users");

        return await _unitOfWork.Users.FindAsync(u => u.IsActive, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deactivating user: {UserId}", userId);

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for deactivation: {UserId}", userId);
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        user.Deactivate();
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deactivated user: {UserId}", userId);
    }
}
