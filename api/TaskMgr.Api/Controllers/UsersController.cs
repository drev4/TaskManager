using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Interfaces;
using AutoMapper;

namespace TaskMgr.Api.Controllers;

/// <summary>
/// Controller for user management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize] // TODO: Enable when authentication is configured
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// Initializes a new instance of the UsersController
    /// </summary>
    /// <param name="userService">User service</param>
    /// <param name="mapper">AutoMapper instance</param>
    /// <param name="logger">Logger instance</param>
    public UsersController(IUserService userService, IMapper mapper, ILogger<UsersController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        var user = await _userService.GetUserByAzureObjectIdAsync(azureObjectId, cancellationToken);

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

        var userDto = _mapper.Map<UserDto>(user);
        return Ok(new ApiResponseDto<UserDto>
        {
            Data = userDto,
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
        var user = await _userService.GetUserByAzureObjectIdAsync(azureObjectId, cancellationToken);

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

        var updatedUser = await _userService.UpdateUserProfileAsync(
            user.Id,
            updateUserDto.DisplayName,
            updateUserDto.Avatar,
            updateUserDto.PreferredLanguage,
            cancellationToken);

        var userDto = _mapper.Map<UserDto>(updatedUser);
        return Ok(new ApiResponseDto<UserDto>
        {
            Data = userDto,
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
        var users = await _userService.GetActiveUsersAsync(cancellationToken);
        var userDtos = _mapper.Map<List<UserDto>>(users);

        return Ok(new ApiResponseDto<List<UserDto>>
        {
            Data = userDtos,
            Success = true,
            Message = $"Retrieved {userDtos.Count} active users"
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

        var existingUser = await _userService.GetUserByAzureObjectIdAsync(azureObjectId, cancellationToken);
        bool isNewUser = existingUser == null;

        var user = await _userService.GetOrCreateUserAsync(azureObjectId, email, displayName, cancellationToken);

        // Record login
        await _userService.RecordUserLoginAsync(azureObjectId, cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        var response = new ApiResponseDto<UserDto>
        {
            Data = userDto,
            Success = true,
            Message = isNewUser ? "User account created successfully" : "User account updated successfully"
        };

        return isNewUser ? StatusCode(StatusCodes.Status201Created, response) : Ok(response);
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
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);

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

        var userDto = _mapper.Map<UserDto>(user);
        return Ok(new ApiResponseDto<UserDto>
        {
            Data = userDto,
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

        if (string.IsNullOrEmpty(azureObjectId))
        {
            _logger.LogWarning("Unable to extract Azure Object ID from token claims");
            throw new UnauthorizedAccessException("Invalid user token - missing object ID");
        }

        return azureObjectId;
    }

    /// <summary>
    /// Gets the current user's email from the JWT token
    /// </summary>
    /// <returns>User email</returns>
    private string GetCurrentUserEmail()
    {
        return User.FindFirst("emails")?.Value
               ?? User.FindFirst("email")?.Value
               ?? User.FindFirst(ClaimTypes.Email)?.Value
               ?? string.Empty;
    }

    /// <summary>
    /// Gets the current user's display name from the JWT token
    /// </summary>
    /// <returns>User display name</returns>
    private string GetCurrentUserDisplayName()
    {
        return User.FindFirst("name")?.Value
               ?? User.FindFirst(ClaimTypes.Name)?.Value
               ?? User.FindFirst("given_name")?.Value
               ?? string.Empty;
    }
}
