using TaskMgr.Api.Domain.Common;
using TaskMgr.Api.Domain.Entities;

namespace TaskMgr.Api.Domain.Events;

public class TaskCreatedEvent : BaseEvent
{
    public TaskItem Task { get; }

    public TaskCreatedEvent(TaskItem task)
    {
        Task = task;
    }
}
