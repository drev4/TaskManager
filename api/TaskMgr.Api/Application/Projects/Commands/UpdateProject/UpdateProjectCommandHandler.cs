using AutoMapper;
using MediatR;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Application.Projects.Commands.UpdateProject;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProjectCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProjectDto?> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetSingleAsync(
            p => p.Id == request.ProjectId && p.OwnerUserId == request.UserId,
            cancellationToken);

        if (project == null)
        {
            return null;
        }

        project.Update(request.Name, request.Description, request.Color, request.DueDate);

        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProjectDto>(project);
    }
}
