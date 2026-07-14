using Moq;
using TaskMgr.Api.Application.Projects.Commands.UpdateProject;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Projects.Commands;

public class UpdateProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_ProjectOwnedByUser_UpdatesAndReturnsDto()
    {
        var builder = new UnitOfWorkMockBuilder();
        var ownerId = Guid.NewGuid();
        var project = new Project("Old name", ownerId);

        builder.Projects
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var handler = new UpdateProjectCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new UpdateProjectCommand { ProjectId = project.Id, UserId = ownerId, Name = "New name" }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New name", result!.Name);
    }

    [Fact]
    public async Task Handle_ProjectNotOwned_ReturnsNull()
    {
        var builder = new UnitOfWorkMockBuilder();
        builder.Projects
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var handler = new UpdateProjectCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new UpdateProjectCommand { ProjectId = Guid.NewGuid(), UserId = Guid.NewGuid() }, CancellationToken.None);

        Assert.Null(result);
    }
}
