using MediatR;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Application.Projects.Commands.UnarchiveProject;

public class UnarchiveProjectCommandHandler : IRequestHandler<UnarchiveProjectCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UnarchiveProjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UnarchiveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetSingleAsync(
            p => p.Id == request.ProjectId && p.OwnerUserId == request.UserId,
            cancellationToken);

        if (project == null)
        {
            return false;
        }

        project.Unarchive();
        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
