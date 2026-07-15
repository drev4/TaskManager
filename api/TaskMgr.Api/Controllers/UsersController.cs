using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using MediatR;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Application.Users.Commands.InitializeUser;
using TaskMgr.Api.Application.Users.Commands.UpdateUserProfile;
using TaskMgr.Api.Application.Users.Queries.GetActiveUsers;
using TaskMgr.Api.Application.Users.Queries.GetCurrentUser;
using TaskMgr.Api.Application.Users.Queries.GetUserById;
using TaskMgr.Api.Infrastructure.Authorization;
using TaskMgr.Api.Options;

namespace TaskMgr.Api.Controllers;

/// <summary>
/// Controller for user management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.ConditionalAuth)]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<UsersController> _logger;
    private readonly IOptionsMonitor<AuthOptions> _authOptions;

    /// <summary>
    /// Initializes a new instance of the UsersController
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="authOptions">Auth feature toggle</param>
    public UsersController(ISender sender, ILogger<UsersController> logger, IOptionsMonitor<AuthOptions> authOptions)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authOptions = authOptions ?? throw new ArgumentNullException(nameof(authOptions));
    }

    /// <summary>
    /// Gets the current user's profile
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current user's profile</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponseDto<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> GetCurrentUser(
        CancellationToken cancellationToken = default)
    {
        var azureObjectId = GetCurrentUserAzureObjectId();
        var user = await _sender.Send(new GetCurrentUserQuery { AzureObjectId = azureObjectId }, cancellationToken);

        if (user == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = "Current user profile was not found.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(new ApiResponseDto<UserDto>
        {
            Data = user,
            Success = true,
            Message = "User profile retrieved successfully"
        });
    }

    /// <summary>
    /// Updates the current user's profile
    /// </summary>
    /// <param name="updateUserDto">User profile update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user profile</returns>
    [HttpPut("me")]
    [ProducesResponseType(typeof(ApiResponseDto<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> UpdateCurrentUser(
        [FromBody] UpdateUserProfileDto updateUserDto,
        CancellationToken cancellationToken = default)
    {
        var azureObjectId = GetCurrentUserAzureObjectId();
        var user = await _sender.Send(new GetCurrentUserQuery { AzureObjectId = azureObjectId }, cancellationToken);

        if (user == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = "Current user profile was not found.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        var updatedUser = await _sender.Send(new UpdateUserProfileCommand
        {
            UserId = user.Id,
            DisplayName = updateUserDto.DisplayName,
            Avatar = updateUserDto.Avatar,
            PreferredLanguage = updateUserDto.PreferredLanguage
        }, cancellationToken);

        return Ok(new ApiResponseDto<UserDto>
        {
            Data = updatedUser,
            Success = true,
            Message = "User profile updated successfully"
        });
    }

    /// <summary>
    /// Gets all active users (for assignment purposes)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<List<UserDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDto<List<UserDto>>>> GetActiveUsers(
        CancellationToken cancellationToken = default)
    {
        var users = await _sender.Send(new GetActiveUsersQuery(), cancellationToken);

        return Ok(new ApiResponseDto<List<UserDto>>
        {
            Data = users,
            Success = true,
            Message = $"Retrieved {users.Count} active users"
        });
    }

    /// <summary>
    /// Initializes or updates the current user based on Azure AD B2C token
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile</returns>
    [HttpPost("initialize")]
    [ProducesResponseType(typeof(ApiResponseDto<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> InitializeUser(
        CancellationToken cancellationToken = default)
    {
        var azureObjectId = GetCurrentUserAzureObjectId();
        var email = GetCurrentUserEmail();
        var displayName = GetCurrentUserDisplayName();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(displayName))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid token",
                Detail = "User email and display name are required in the token.",
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }

        var result = await _sender.Send(new InitializeUserCommand
        {
            AzureObjectId = azureObjectId,
            Email = email,
            DisplayName = displayName
        }, cancellationToken);

        var response = new ApiResponseDto<UserDto>
        {
            Data = result.User,
            Success = true,
            Message = result.IsNew ? "User account created successfully" : "User account updated successfully"
        };

        return result.IsNew ? StatusCode(StatusCodes.Status201Created, response) : Ok(response);
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseDto<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> GetUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _sender.Send(new GetUserByIdQuery { UserId = id }, cancellationToken);

        if (user == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "User not found",
                Detail = $"User with ID {id} was not found.",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(new ApiResponseDto<UserDto>
        {
            Data = user,
            Success = true,
            Message = "User retrieved successfully"
        });
    }

    /// <summary>
    /// Gets the current user's Azure Object ID from the JWT token
    /// </summary>
    /// <returns>Azure Object ID</returns>
    private string GetCurrentUserAzureObjectId()
    {
        var azureObjectId = User.FindFirst("oid")?.Value
                           ?? User.FindFirst("sub")?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(azureObjectId))
        {
            return azureObjectId;
        }

        if (!_authOptions.CurrentValue.Enabled)
        {
            // Development mode: Bob Wilson's AzureObjectId from seed-data.sql
            _logger.LogInformation("No user authentication found, using mock user for development");
            return "dev-azure-id-3";
        }

        _logger.LogWarning("Unable to extract Azure Object ID from token claims");
        throw new UnauthorizedAccessException("Invalid user token - missing object ID");
    }

    /// <summary>
    /// Gets the current user's email from the JWT token
    /// </summary>
    /// <returns>User email</returns>
    private string GetCurrentUserEmail()
    {
        var email = User.FindFirst("emails")?.Value
                   ?? User.FindFirst("email")?.Value
                   ?? User.FindFirst(ClaimTypes.Email)?.Value;

        if (!string.IsNullOrEmpty(email))
        {
            return email;
        }

        // Development mode: Bob Wilson's email from seed-data.sql
        return !_authOptions.CurrentValue.Enabled ? "bob.wilson@example.com" : string.Empty;
    }

    /// <summary>
    /// Gets the current user's display name from the JWT token
    /// </summary>
    /// <returns>User display name</returns>
    private string GetCurrentUserDisplayName()
    {
        var displayName = User.FindFirst("name")?.Value
                         ?? User.FindFirst(ClaimTypes.Name)?.Value
                         ?? User.FindFirst("given_name")?.Value;

        if (!string.IsNullOrEmpty(displayName))
        {
            return displayName;
        }

        // Development mode: Bob Wilson's display name from seed-data.sql
        return !_authOptions.CurrentValue.Enabled ? "Bob Wilson" : string.Empty;
    }
}
