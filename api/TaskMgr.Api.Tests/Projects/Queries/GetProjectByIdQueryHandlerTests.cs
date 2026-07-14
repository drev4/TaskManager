using Moq;
using TaskMgr.Api.Application.Projects.Queries.GetProjectById;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Projects.Queries;

public class GetProjectByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_IncludeTasksTrue_PopulatesProjectTasks()
    {
        var builder = new UnitOfWorkMockBuilder();
        var ownerId = Guid.NewGuid();
        var project = new Project("Website", ownerId);
        var tasks = new List<TaskItem> { new("Task 1", project.Id) };

        builder.Projects
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        builder.Tasks
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var handler = new GetProjectByIdQueryHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new GetProjectByIdQuery { ProjectId = project.Id, UserId = ownerId, IncludeTasks = true }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result!.TotalTasksCount);
    }

    [Fact]
    public async Task Handle_ProjectNotOwned_ReturnsNull()
    {
        var builder = new UnitOfWorkMockBuilder();
        builder.Projects
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var handler = new GetProjectByIdQueryHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new GetProjectByIdQuery { ProjectId = Guid.NewGuid(), UserId = Guid.NewGuid() }, CancellationToken.None);

        Assert.Null(result);
    }
}
