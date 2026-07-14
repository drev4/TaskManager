using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediatR;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Application.Tasks.Queries.GetDashboardStats;

namespace TaskMgr.Api.Controllers;

/// <summary>
/// Controller for dashboard and analytics operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[Authorize] // TODO: Enable when authentication is configured
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class DashboardController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<DashboardController> _logger;

    /// <summary>
    /// Initializes a new instance of the DashboardController
    /// </summary>
    /// <param name="sender">MediatR sender</param>
    /// <param name="logger">Logger instance</param>
    public DashboardController(ISender sender, ILogger<DashboardController> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets dashboard statistics for the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard statistics</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponseDto<DashboardStatsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDto<DashboardStatsDto>>> GetDashboardStats(
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var stats = await _sender.Send(new GetDashboardStatsQuery { UserId = userId }, cancellationToken);

        return Ok(new ApiResponseDto<DashboardStatsDto>
        {
            Data = stats,
            Success = true,
            Message = "Dashboard statistics retrieved successfully"
        });
    }

    /// <summary>
    /// Gets recent activity for the current user
    /// </summary>
    /// <param name="limit">Maximum number of activities to return (default: 10, max: 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recent activity list</returns>
    [HttpGet("activity")]
    [ProducesResponseType(typeof(ApiResponseDto<List<RecentActivityDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDto<List<RecentActivityDto>>>> GetRecentActivity(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        // Validate limit parameter
        if (limit < 1) limit = 10;
        if (limit > 50) limit = 50;

        var userId = GetCurrentUserId();

        // For now, return mock data - in a real implementation, you'd have an activity tracking system
        var activities = GetMockRecentActivity(userId, limit);

        return Ok(new ApiResponseDto<List<RecentActivityDto>>
        {
            Data = activities,
            Success = true,
            Message = $"Retrieved {activities.Count} recent activities"
        });
    }

    /// <summary>
    /// Gets task completion trends for the current user
    /// </summary>
    /// <param name="days">Number of days to include in the trend (default: 30)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completion trend data</returns>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(ApiResponseDto<Dictionary<string, object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDto<Dictionary<string, object>>>> GetCompletionTrends(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        // Validate days parameter
        if (days < 7) days = 7;
        if (days > 365) days = 365;

        var userId = GetCurrentUserId();

        // For now, return mock trend data
        var trends = GetMockTrendData(days);

        return Ok(new ApiResponseDto<Dictionary<string, object>>
        {
            Data = trends,
            Success = true,
            Message = $"Retrieved completion trends for {days} days"
        });
    }

    /// <summary>
    /// Gets productivity metrics for the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Productivity metrics</returns>
    [HttpGet("productivity")]
    [ProducesResponseType(typeof(ApiResponseDto<Dictionary<string, object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponseDto<Dictionary<string, object>>>> GetProductivityMetrics(
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        // For now, return mock productivity data
        var metrics = GetMockProductivityMetrics();

        return Ok(new ApiResponseDto<Dictionary<string, object>>
        {
            Data = metrics,
            Success = true,
            Message = "Productivity metrics retrieved successfully"
        });
    }

    /// <summary>
    /// Gets the current user ID from the JWT token
    /// </summary>
    /// <returns>Current user ID</returns>
    private Guid GetCurrentUserId()
    {
        // TODO: Remove this mock user when authentication is enabled
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value
                         ?? User.FindFirst("oid")?.Value;

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        // Development mode: return Bob Wilson's user ID from seed data
        _logger.LogInformation("No user authentication found, using mock user for development");
        return Guid.Parse("2C6D7627-738C-4D39-9644-E7B703332DC5");
    }

    /// <summary>
    /// Gets mock recent activity data
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="limit">Number of activities to return</param>
    /// <returns>Mock activity list</returns>
    private static List<RecentActivityDto> GetMockRecentActivity(Guid userId, int limit)
    {
        var activities = new List<RecentActivityDto>();
        var random = new Random();
        var activityTypes = new[] { "task_created", "task_completed", "project_created", "task_assigned" };
        var descriptions = new[]
        {
            "Created task 'Implement user authentication'",
            "Completed task 'Design homepage mockup'",
            "Created project 'Website Redesign'",
            "Assigned task 'Setup development environment'",
            "Completed task 'Write API documentation'",
            "Created task 'Add error handling middleware'"
        };

        for (int i = 0; i < limit; i++)
        {
            activities.Add(new RecentActivityDto
            {
                Id = Guid.NewGuid(),
                Type = activityTypes[random.Next(activityTypes.Length)],
                Description = descriptions[random.Next(descriptions.Length)],
                Timestamp = DateTime.UtcNow.AddHours(-random.Next(1, 72)),
                User = new UserDto
                {
                    Id = userId,
                    DisplayName = "Current User",
                    Email = "user@example.com"
                },
                RelatedItem = new RelatedItemDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Sample Item",
                    Type = "task"
                }
            });
        }

        return activities.OrderByDescending(a => a.Timestamp).ToList();
    }

    /// <summary>
    /// Gets mock trend data
    /// </summary>
    /// <param name="days">Number of days</param>
    /// <returns>Mock trend data</returns>
    private static Dictionary<string, object> GetMockTrendData(int days)
    {
        var random = new Random();
        var dates = new List<string>();
        var completed = new List<int>();
        var created = new List<int>();

        for (int i = days - 1; i >= 0; i--)
        {
            var date = DateTime.UtcNow.AddDays(-i);
            dates.Add(date.ToString("yyyy-MM-dd"));
            completed.Add(random.Next(0, 8));
            created.Add(random.Next(1, 10));
        }

        return new Dictionary<string, object>
        {
            ["dates"] = dates,
            ["tasksCompleted"] = completed,
            ["tasksCreated"] = created,
            ["averageCompletionRate"] = Math.Round(completed.Average(), 1),
            ["totalCompleted"] = completed.Sum(),
            ["totalCreated"] = created.Sum()
        };
    }

    /// <summary>
    /// Gets mock productivity metrics
    /// </summary>
    /// <returns>Mock productivity data</returns>
    private static Dictionary<string, object> GetMockProductivityMetrics()
    {
        var random = new Random();
        
        return new Dictionary<string, object>
        {
            ["averageTasksPerDay"] = Math.Round(random.NextDouble() * 5 + 2, 1),
            ["averageCompletionTime"] = Math.Round(random.NextDouble() * 3 + 1, 1), // days
            ["mostProductiveHour"] = random.Next(9, 17),
            ["streakDays"] = random.Next(1, 15),
            ["focusScore"] = Math.Round(random.NextDouble() * 40 + 60, 1), // 60-100
            ["priorityDistribution"] = new Dictionary<string, int>
            {
                ["urgent"] = random.Next(5, 15),
                ["high"] = random.Next(15, 30),
                ["medium"] = random.Next(40, 60),
                ["low"] = random.Next(10, 25)
            },
            ["weeklyGoalCompletion"] = Math.Round(random.NextDouble() * 30 + 70, 1) // 70-100%
        };
    }
}
