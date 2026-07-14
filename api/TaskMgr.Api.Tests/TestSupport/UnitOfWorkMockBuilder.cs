using Moq;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Tests.TestSupport;

/// <summary>
/// Wires up a mocked IUnitOfWork with per-entity repository mocks so handler tests
/// only need to stub the repository calls relevant to the scenario under test.
/// </summary>
public class UnitOfWorkMockBuilder
{
    public Mock<IRepository<TaskItem>> Tasks { get; } = new();
    public Mock<IRepository<Project>> Projects { get; } = new();
    public Mock<IRepository<User>> Users { get; } = new();
    public Mock<IUnitOfWork> UnitOfWork { get; } = new();

    public UnitOfWorkMockBuilder()
    {
        UnitOfWork.SetupGet(u => u.Tasks).Returns(Tasks.Object);
        UnitOfWork.SetupGet(u => u.Projects).Returns(Projects.Object);
        UnitOfWork.SetupGet(u => u.Users).Returns(Users.Object);
        UnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }
}
