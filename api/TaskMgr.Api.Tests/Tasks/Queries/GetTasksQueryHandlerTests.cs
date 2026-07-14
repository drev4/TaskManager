using Moq;
using TaskMgr.Api.Application.Tasks.Queries.GetTasks;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Tasks.Queries;

public class GetTasksQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedTasksWithPaginationMetadata()
    {
        var builder = new UnitOfWorkMockBuilder();
        var projectId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            new("Task 1", projectId),
            new("Task 2", projectId)
        };

        builder.Tasks
            .Setup(r => r.GetPagedAsync(
                1, 20,
                It.IsAny<System.Linq.Expressions.Expression<Func<TaskItem, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<TaskItem, object>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((tasks, 2));

        var handler = new GetTasksQueryHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new GetTasksQuery { UserId = Guid.NewGuid(), PageNumber = 1, PageSize = 20 }, CancellationToken.None);

        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.Pagination.Total);
        Assert.False(result.Pagination.HasNext);
        Assert.False(result.Pagination.HasPrevious);
    }
}
