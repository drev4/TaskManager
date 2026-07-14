using Moq;
using TaskMgr.Api.Application.Projects.Commands.DeleteProject;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Projects.Commands;

public class DeleteProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_NoActiveTasks_RemovesProject()
    {
        var builder = new UnitOfWorkMockBuilder();
        var ownerId = Guid.NewGuid();
        var project = new Project("Website", ownerId);

        builder.Projects
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        builder.Tasks
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new DeleteProjectCommandHandler(builder.UnitOfWork.Object);

        var result = await handler.Handle(new DeleteProjectCommand { ProjectId = project.Id, UserId = ownerId }, CancellationToken.None);

        Assert.True(result);
        builder.Projects.Verify(r => r.Remove(project), Times.Once);
    }

    [Fact]
    public async Task Handle_HasActiveTasks_ThrowsInvalidOperationException()
    {
        var builder = new UnitOfWorkMockBuilder();
        var ownerId = Guid.NewGuid();
        var project = new Project("Website", ownerId);

        builder.Projects
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        builder.Tasks
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new DeleteProjectCommandHandler(builder.UnitOfWork.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new DeleteProjectCommand { ProjectId = project.Id, UserId = ownerId }, CancellationToken.None));
    }
}
