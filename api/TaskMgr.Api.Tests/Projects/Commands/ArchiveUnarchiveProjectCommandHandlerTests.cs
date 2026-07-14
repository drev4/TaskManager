using Moq;
using TaskMgr.Api.Application.Projects.Commands.ArchiveProject;
using TaskMgr.Api.Application.Projects.Commands.UnarchiveProject;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Projects.Commands;

public class ArchiveUnarchiveProjectCommandHandlerTests
{
    [Fact]
    public async Task Archive_ProjectOwnedByUser_SetsIsArchivedTrue()
    {
        var builder = new UnitOfWorkMockBuilder();
        var ownerId = Guid.NewGuid();
        var project = new Project("Website", ownerId);

        builder.Projects
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var handler = new ArchiveProjectCommandHandler(builder.UnitOfWork.Object);

        var result = await handler.Handle(new ArchiveProjectCommand { ProjectId = project.Id, UserId = ownerId }, CancellationToken.None);

        Assert.True(result);
        Assert.True(project.IsArchived);
    }

    [Fact]
    public async Task Unarchive_ProjectOwnedByUser_SetsIsArchivedFalse()
    {
        var builder = new UnitOfWorkMockBuilder();
        var ownerId = Guid.NewGuid();
        var project = new Project("Website", ownerId);
        project.Archive();

        builder.Projects
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var handler = new UnarchiveProjectCommandHandler(builder.UnitOfWork.Object);

        var result = await handler.Handle(new UnarchiveProjectCommand { ProjectId = project.Id, UserId = ownerId }, CancellationToken.None);

        Assert.True(result);
        Assert.False(project.IsArchived);
    }

    [Fact]
    public async Task Archive_ProjectNotFound_ReturnsFalse()
    {
        var builder = new UnitOfWorkMockBuilder();
        builder.Projects
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var handler = new ArchiveProjectCommandHandler(builder.UnitOfWork.Object);

        var result = await handler.Handle(new ArchiveProjectCommand { ProjectId = Guid.NewGuid(), UserId = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result);
    }
}
