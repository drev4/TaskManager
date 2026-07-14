using Moq;
using TaskMgr.Api.Application.Tasks.Commands.UpdateTask;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Domain.Enums;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Tasks.Commands;

public class UpdateTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_TaskFound_UpdatesAndReturnsDto()
    {
        var builder = new UnitOfWorkMockBuilder();
        var ownerId = Guid.NewGuid();
        var project = new Project("Website", ownerId);
        var task = new TaskItem("Old title", project.Id, TaskPriority.Low);

        builder.Tasks
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var handler = new UpdateTaskCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var command = new UpdateTaskCommand
        {
            TaskId = task.Id,
            UserId = ownerId,
            Title = "New title",
            Status = Domain.Enums.TaskStatus.Done
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New title", result!.Title);
        Assert.Equal(Domain.Enums.TaskStatus.Done, result.Status);
        builder.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TaskNotFound_ReturnsNull()
    {
        var builder = new UnitOfWorkMockBuilder();
        builder.Tasks
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TaskItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var handler = new UpdateTaskCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create());

        var result = await handler.Handle(new UpdateTaskCommand { TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() }, CancellationToken.None);

        Assert.Null(result);
    }
}
