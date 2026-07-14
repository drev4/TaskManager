using Moq;
using TaskMgr.Api.Application.Tasks.Queries.GetTaskById;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Tasks.Queries;

public class GetTaskByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_TaskFound_ReturnsDto()
    {
        var builder = new UnitOfWorkMockBuilder();
        var task = new TaskItem("Task", Guid.NewGuid());

        builder.Tasks
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var handler = new GetTaskByIdQueryHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new GetTaskByIdQuery { TaskId = task.Id, UserId = Guid.NewGuid() }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(task.Id, result!.Id);
    }

    [Fact]
    public async Task Handle_TaskNotAccessible_ReturnsNull()
    {
        var builder = new UnitOfWorkMockBuilder();
        builder.Tasks
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var handler = new GetTaskByIdQueryHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new GetTaskByIdQuery { TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() }, CancellationToken.None);

        Assert.Null(result);
    }
}
