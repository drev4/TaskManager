using MediatR;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Application.Tasks.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardStatsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var projects = await _unitOfWork.Projects.FindAsync(p => p.OwnerUserId == request.UserId && !p.IsArchived, cancellationToken);
        var projectIds = projects.Select(p => p.Id).ToList();

        var allTasks = await _unitOfWork.Tasks.FindAsync(t => projectIds.Contains(t.ProjectId), cancellationToken);

        var now = DateTime.UtcNow;
        var weekStart = now.AddDays(-(int)now.DayOfWeek);

        var stats = new DashboardStatsDto
        {
            TotalProjects = projects.Count,
            TotalTasks = allTasks.Count,
            CompletedTasks = allTasks.Count(t => t.Status == Domain.Enums.TaskStatus.Done),
            PendingTasks = allTasks.Count(t => t.Status != Domain.Enums.TaskStatus.Done),
            OverdueTasks = allTasks.Count(t => t.IsOverdue()),
            TasksCreatedThisWeek = allTasks.Count(t => t.CreatedAt >= weekStart),
            TasksCompletedThisWeek = allTasks.Count(t => t.CompletedAt >= weekStart)
        };

        stats.CompletionPercentage = stats.TotalTasks > 0
            ? Math.Round((decimal)stats.CompletedTasks / stats.TotalTasks * 100, 1)
            : 0;

        return stats;
    }
}
