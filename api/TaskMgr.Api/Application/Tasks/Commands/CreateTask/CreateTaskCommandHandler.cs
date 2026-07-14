using AutoMapper;
using MediatR;
using TaskMgr.Api.Application.Common;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Domain.Events;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Application.Tasks.Commands.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskItemDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;

    public CreateTaskCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _publisher = publisher;
    }

    public async Task<TaskItemDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate Project and User access
        var project = await _unitOfWork.Projects.GetSingleAsync(
            p => p.Id == request.ProjectId && p.OwnerUserId == request.UserId,
            cancellationToken);

        if (project == null)
        {
            throw new InvalidOperationException($"Project with ID {request.ProjectId} not found or not accessible");
        }

        // 2. Validate Assigned User
        if (request.AssignedToUserId.HasValue)
        {
            var assignedUser = await _unitOfWork.Users.GetByIdAsync(request.AssignedToUserId.Value, cancellationToken);
            if (assignedUser == null || !assignedUser.IsActive)
            {
                throw new InvalidOperationException($"Assigned user with ID {request.AssignedToUserId} not found or inactive");
            }
        }

        // 3. Create Task Entity
        var task = new TaskItem(request.Title, request.ProjectId, request.Priority, request.Description);
        
        if (request.AssignedToUserId.HasValue)
            task.AssignTo(request.AssignedToUserId.Value);
        
        if (request.DueDate.HasValue)
            task.Update(dueDate: request.DueDate);
        
        if (request.EstimatedHours.HasValue)
            task.Update(estimatedHours: request.EstimatedHours);
        
        if (request.Tags.Any())
            task.SetTags(request.Tags);

        // 4. Add Domain Event
        task.AddDomainEvent(new TaskCreatedEvent(task));

        // 5. Persist
        await _unitOfWork.Tasks.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Dispatch Domain Events
        await DomainEventDispatcher.PublishAndClearAsync(_publisher, task, cancellationToken);

        // 7. Return DTO
        return _mapper.Map<TaskItemDto>(task);
    }
}
