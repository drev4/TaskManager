using Moq;
using TaskMgr.Api.Application.Tasks.Queries.GetDashboardStats;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Tasks.Queries;

public class GetDashboardStatsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ComputesCompletionPercentageAcrossOwnedProjects()
    {
        var builder = new UnitOfWorkMockBuilder();
        var userId = Guid.NewGuid();
        var project = new Project("Website", userId);

        var doneTask = new TaskItem("Done", project.Id);
        doneTask.UpdateStatus(Domain.Enums.TaskStatus.Done);
        var pendingTask = new TaskItem("Pending", project.Id);

        builder.Projects
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Project> { project });

        builder.Tasks
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem> { doneTask, pendingTask });

        var handler = new GetDashboardStatsQueryHandler(builder.UnitOfWork.Object);

        var result = await handler.Handle(new GetDashboardStatsQuery { UserId = userId }, CancellationToken.None);

        Assert.Equal(1, result.TotalProjects);
        Assert.Equal(2, result.TotalTasks);
        Assert.Equal(1, result.CompletedTasks);
        Assert.Equal(50m, result.CompletionPercentage);
    }
}
