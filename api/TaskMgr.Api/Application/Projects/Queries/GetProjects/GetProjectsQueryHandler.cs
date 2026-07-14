using AutoMapper;
using MediatR;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Application.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProjectsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await _unitOfWork.Projects.FindAsync(
            p => p.OwnerUserId == request.UserId && (request.IncludeArchived || !p.IsArchived),
            cancellationToken);

        return _mapper.Map<List<ProjectDto>>(projects);
    }
}
