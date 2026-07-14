using Moq;
using TaskMgr.Api.Application.Tasks.Commands.RecordTaskTime;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Tasks.Commands;

public class RecordTaskTimeCommandHandlerTests
{
    [Fact]
    public async Task Handle_TaskFound_AccumulatesActualHours()
    {
        var builder = new UnitOfWorkMockBuilder();
        var task = new TaskItem("Task", Guid.NewGuid());

        builder.Tasks
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var handler = new RecordTaskTimeCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new RecordTaskTimeCommand { TaskId = task.Id, UserId = Guid.NewGuid(), Hours = 2.5m }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2.5m, result!.ActualHours);
    }

    [Fact]
    public async Task Handle_TaskNotAccessible_ReturnsNull()
    {
        var builder = new UnitOfWorkMockBuilder();
        builder.Tasks
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var handler = new RecordTaskTimeCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new RecordTaskTimeCommand { TaskId = Guid.NewGuid(), UserId = Guid.NewGuid(), Hours = 1 }, CancellationToken.None);

        Assert.Null(result);
    }
}
