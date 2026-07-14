using Moq;
using TaskMgr.Api.Application.Tasks.Commands.DeleteTask;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Tasks.Commands;

public class DeleteTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_TaskOwnedByUser_RemovesTaskAndReturnsTrue()
    {
        var builder = new UnitOfWorkMockBuilder();
        var task = new TaskItem("Task", Guid.NewGuid());

        builder.Tasks
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var handler = new DeleteTaskCommandHandler(builder.UnitOfWork.Object);

        var result = await handler.Handle(new DeleteTaskCommand { TaskId = task.Id, UserId = Guid.NewGuid() }, CancellationToken.None);

        Assert.True(result);
        builder.Tasks.Verify(r => r.Remove(task), Times.Once);
        builder.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TaskNotFound_ReturnsFalse()
    {
        var builder = new UnitOfWorkMockBuilder();
        builder.Tasks
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var handler = new DeleteTaskCommandHandler(builder.UnitOfWork.Object);

        var result = await handler.Handle(new DeleteTaskCommand { TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result);
        builder.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
