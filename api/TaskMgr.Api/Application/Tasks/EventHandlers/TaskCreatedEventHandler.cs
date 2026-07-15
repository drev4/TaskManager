using MediatR;
using Microsoft.AspNetCore.SignalR;
using TaskMgr.Api.Domain.Events;
using TaskMgr.Api.Infrastructure.Hubs;
using TaskMgr.Api.Application.DTOs;
using AutoMapper;

namespace TaskMgr.Api.Application.Tasks.EventHandlers;

public class TaskCreatedEventHandler : INotificationHandler<TaskCreatedEvent>
{
    private readonly IHubContext<TaskHub> _hubContext;
    private readonly IMapper _mapper;

    public TaskCreatedEventHandler(IHubContext<TaskHub> hubContext, IMapper mapper)
    {
        _hubContext = hubContext;
        _mapper = mapper;
    }

    public async Task Handle(TaskCreatedEvent notification, CancellationToken cancellationToken)
    {
        var taskDto = _mapper.Map<TaskItemDto>(notification.Task);

        // Notify the owner
        await _hubContext.Clients.Group($"User_{notification.OwnerUserId}")
            .SendAsync("TaskCreated", taskDto, cancellationToken);

        // Notify the assigned user if different
        if (notification.Task.AssignedToUserId.HasValue &&
            notification.Task.AssignedToUserId != notification.OwnerUserId)
        {
            await _hubContext.Clients.Group($"User_{notification.Task.AssignedToUserId}")
                .SendAsync("TaskCreated", taskDto, cancellationToken);
        }
    }
}
