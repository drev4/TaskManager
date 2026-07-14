using System.Linq.Expressions;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Entities;

namespace TaskMgr.Api.Application.Tasks;

/// <summary>
/// Builds the EF-translatable filter expression shared by task queries
/// </summary>
public static class TaskFilterExpressionBuilder
{
    public static Expression<Func<TaskItem, bool>> Build(Guid userId, TaskFilterDto? filter)
    {
        var searchTerm = filter?.SearchTerm?.ToLower();

        return t =>
            (t.Project.OwnerUserId == userId || t.AssignedToUserId == userId) &&
            (filter == null || !filter.ProjectId.HasValue || t.ProjectId == filter.ProjectId.Value) &&
            (filter == null || !filter.Status.HasValue || t.Status == filter.Status.Value) &&
            (filter == null || !filter.Priority.HasValue || t.Priority == filter.Priority.Value) &&
            (filter == null || !filter.AssignedToUserId.HasValue || t.AssignedToUserId == filter.AssignedToUserId.Value) &&
            (filter == null || !filter.DueBefore.HasValue || t.DueDate <= filter.DueBefore.Value) &&
            (filter == null || !filter.DueAfter.HasValue || t.DueDate >= filter.DueAfter.Value) &&
            (filter == null || !filter.OverdueOnly.HasValue || !filter.OverdueOnly.Value ||
                (t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow && t.Status != Domain.Enums.TaskStatus.Done)) &&
            (string.IsNullOrWhiteSpace(searchTerm) ||
                (t.Title.ToLower().Contains(searchTerm!) || (t.Description != null && t.Description.ToLower().Contains(searchTerm!))));
    }
}
