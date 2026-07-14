using Moq;
using TaskMgr.Api.Application.Users.Commands.UpdateUserProfile;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Users.Commands;

public class UpdateUserProfileCommandHandlerTests
{
    [Fact]
    public async Task Handle_ActiveUser_UpdatesProfile()
    {
        var builder = new UnitOfWorkMockBuilder();
        var user = new User("user@example.com", "Old Name", "azure-oid");

        builder.Users.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = new UpdateUserProfileCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new UpdateUserProfileCommand { UserId = user.Id, DisplayName = "New Name" }, CancellationToken.None);

        Assert.Equal("New Name", result.DisplayName);
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsInvalidOperationException()
    {
        var builder = new UnitOfWorkMockBuilder();
        var user = new User("user@example.com", "Old Name", "azure-oid");
        user.Deactivate();

        builder.Users.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = new UpdateUserProfileCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new UpdateUserProfileCommand { UserId = user.Id, DisplayName = "New Name" }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
    {
        var builder = new UnitOfWorkMockBuilder();
        builder.Users.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var handler = new UpdateUserProfileCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(new UpdateUserProfileCommand { UserId = Guid.NewGuid() }, CancellationToken.None));
    }
}
