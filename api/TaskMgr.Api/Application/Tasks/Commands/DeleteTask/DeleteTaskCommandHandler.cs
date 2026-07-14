using MediatR;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Application.Tasks.Commands.DeleteTask;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetSingleAsync(
            t => t.Id == request.TaskId && t.Project.OwnerUserId == request.UserId,
            cancellationToken);

        if (task == null)
        {
            return false;
        }

        _unitOfWork.Tasks.Remove(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
