using Moq;
using TaskMgr.Api.Application.Users.Commands.InitializeUser;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Users.Commands;

public class InitializeUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_NewAzureObjectId_CreatesUserAndReportsIsNew()
    {
        var builder = new UnitOfWorkMockBuilder();
        builder.Users
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new InitializeUserCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new InitializeUserCommand
        {
            AzureObjectId = "azure-oid",
            Email = "new@example.com",
            DisplayName = "New User"
        }, CancellationToken.None);

        Assert.True(result.IsNew);
        Assert.Equal("New User", result.User.DisplayName);
        builder.Users.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingAzureObjectId_UpdatesProfileAndRecordsLogin()
    {
        var builder = new UnitOfWorkMockBuilder();
        var existingUser = new User("existing@example.com", "Old Name", "azure-oid");

        builder.Users
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new InitializeUserCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new InitializeUserCommand
        {
            AzureObjectId = "azure-oid",
            Email = "existing@example.com",
            DisplayName = "Updated Name"
        }, CancellationToken.None);

        Assert.False(result.IsNew);
        Assert.Equal("Updated Name", result.User.DisplayName);
        Assert.NotNull(existingUser.LastLoginAt);
        builder.Users.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
