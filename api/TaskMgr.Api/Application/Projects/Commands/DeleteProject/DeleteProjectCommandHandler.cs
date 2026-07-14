using MediatR;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Application.Projects.Commands.DeleteProject;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetSingleAsync(
            p => p.Id == request.ProjectId && p.OwnerUserId == request.UserId,
            cancellationToken);

        if (project == null)
        {
            return false;
        }

        var hasActiveTasks = await _unitOfWork.Tasks.AnyAsync(
            t => t.ProjectId == request.ProjectId && t.Status != Domain.Enums.TaskStatus.Done,
            cancellationToken);

        if (hasActiveTasks)
        {
            throw new InvalidOperationException("Cannot delete project with active tasks. Complete or delete all tasks first.");
        }

        _unitOfWork.Projects.Remove(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
