using Moq;
using TaskMgr.Api.Application.Projects.Commands.CreateProject;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Projects.Commands;

public class CreateProjectCommandHandlerTests
{
    [Fact]
    public async Task Handle_ActiveUser_CreatesProject()
    {
        var builder = new UnitOfWorkMockBuilder();
        var user = new User("owner@example.com", "Owner", "azure-oid");

        builder.Users.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = new CreateProjectCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new CreateProjectCommand { Name = "Website", UserId = user.Id }, CancellationToken.None);

        Assert.Equal("Website", result.Name);
        builder.Projects.Verify(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsInvalidOperationException()
    {
        var builder = new UnitOfWorkMockBuilder();
        var user = new User("owner@example.com", "Owner", "azure-oid");
        user.Deactivate();

        builder.Users.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = new CreateProjectCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new CreateProjectCommand { Name = "Website", UserId = user.Id }, CancellationToken.None));
    }
}
