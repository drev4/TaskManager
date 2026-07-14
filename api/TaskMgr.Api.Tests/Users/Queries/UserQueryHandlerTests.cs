using Moq;
using TaskMgr.Api.Application.Users.Queries.GetActiveUsers;
using TaskMgr.Api.Application.Users.Queries.GetCurrentUser;
using TaskMgr.Api.Application.Users.Queries.GetUserById;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Users.Queries;

public class UserQueryHandlerTests
{
    [Fact]
    public async Task GetCurrentUser_ActiveUserWithMatchingAzureObjectId_ReturnsDto()
    {
        var builder = new UnitOfWorkMockBuilder();
        var user = new User("user@example.com", "User", "azure-oid");

        builder.Users
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new GetCurrentUserQueryHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new GetCurrentUserQuery { AzureObjectId = "azure-oid" }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result!.Id);
    }

    [Fact]
    public async Task GetUserById_NoMatch_ReturnsNull()
    {
        var builder = new UnitOfWorkMockBuilder();
        builder.Users
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new GetUserByIdQueryHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new GetUserByIdQuery { UserId = Guid.NewGuid() }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveUsers_ReturnsMappedList()
    {
        var builder = new UnitOfWorkMockBuilder();
        var users = new List<User> { new("a@example.com", "A", "oid-a"), new("b@example.com", "B", "oid-b") };

        builder.Users
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var handler = new GetActiveUsersQueryHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new GetActiveUsersQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }
}
