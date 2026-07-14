using Moq;
using TaskMgr.Api.Application.Tasks.Commands.CreateTask;
using TaskMgr.Api.Domain.Common;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Domain.Enums;
using TaskMgr.Api.Tests.TestSupport;
using Xunit;

namespace TaskMgr.Api.Tests.Tasks.Commands;

public class CreateTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesTaskAndPublishesDomainEvent()
    {
        var builder = new UnitOfWorkMockBuilder();
        var ownerId = Guid.NewGuid();
        var project = new Project("Website", ownerId);

        builder.Projects
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var publisher = new Mock<MediatR.IPublisher>();
        var handler = new CreateTaskCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create(), publisher.Object);

        var command = new CreateTaskCommand
        {
            Title = "Design homepage",
            ProjectId = project.Id,
            Priority = TaskPriority.High,
            UserId = ownerId
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Design homepage", result.Title);
        builder.Tasks.Verify(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
        builder.UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        publisher.Verify(p => p.Publish(It.IsAny<BaseEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProjectNotOwnedByUser_ThrowsInvalidOperationException()
    {
        var builder = new UnitOfWorkMockBuilder();
        builder.Projects
            .Setup(r => r.GetSingleAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        var handler = new CreateTaskCommandHandler(builder.UnitOfWork.Object, MapperFactory.Create(), Mock.Of<MediatR.IPublisher>());

        var command = new CreateTaskCommand { Title = "Task", ProjectId = Guid.NewGuid(), UserId = Guid.NewGuid() };

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }
}
