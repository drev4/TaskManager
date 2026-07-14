using Moq;
using TaskMgr.Api.Application.Projects.Queries.GetProjects;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Projects.Queries;

public class GetProjectsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsProjectsForUser()
    {
        var builder = new UnitOfWorkMockBuilder();
        var userId = Guid.NewGuid();
        var projects = new List<Project> { new("Website", userId), new("Mobile app", userId) };

        builder.Projects
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projects);

        var handler = new GetProjectsQueryHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new GetProjectsQuery { UserId = userId }, CancellationToken.None);

        Assert.Equal(2, result.Count);
    }
}
