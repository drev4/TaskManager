using AutoMapper;
using MediatR;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Application.Tasks.Commands.UpdateTask;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskItemDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateTaskCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TaskItemDto?> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetSingleAsync(
            t => t.Id == request.TaskId &&
                 (t.Project.OwnerUserId == request.UserId || t.AssignedToUserId == request.UserId),
            cancellationToken);

        if (task == null)
        {
            return null;
        }

        task.Update(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.EstimatedHours);

        if (request.Status.HasValue)
            task.UpdateStatus(request.Status.Value);

        if (request.AssignedToUserId.HasValue)
        {
            if (request.AssignedToUserId == Guid.Empty)
            {
                task.Unassign();
            }
            else
            {
                var assignedUser = await _unitOfWork.Users.GetByIdAsync(request.AssignedToUserId.Value, cancellationToken);
                if (assignedUser == null || !assignedUser.IsActive)
                {
                    throw new InvalidOperationException($"Assigned user with ID {request.AssignedToUserId} not found or inactive");
                }

                task.AssignTo(request.AssignedToUserId.Value);
            }
        }

        if (request.Tags != null)
            task.SetTags(request.Tags);

        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TaskItemDto>(task);
    }
}
