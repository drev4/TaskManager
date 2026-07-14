using AutoMapper;
using MediatR;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Application.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProjectByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetSingleAsync(
            p => p.Id == request.ProjectId && p.OwnerUserId == request.UserId,
            cancellationToken);

        if (project == null)
        {
            return null;
        }

        if (request.IncludeTasks)
        {
            var tasks = await _unitOfWork.Tasks.FindAsync(t => t.ProjectId == request.ProjectId, cancellationToken);
            project.Tasks.Clear();
            foreach (var task in tasks)
            {
                project.Tasks.Add(task);
            }
        }

        return _mapper.Map<ProjectDto>(project);
    }
}
