using TaskMgr.Api.Domain.Common;
using TaskMgr.Api.Domain.Entities;

namespace TaskMgr.Api.Domain.Events;

public class TaskCreatedEvent : BaseEvent
{
    public TaskItem Task { get; }
    public Guid OwnerUserId { get; }

    public TaskCreatedEvent(TaskItem task, Guid ownerUserId)
    {
        Task = task;
        OwnerUserId = ownerUserId;
    }
}
