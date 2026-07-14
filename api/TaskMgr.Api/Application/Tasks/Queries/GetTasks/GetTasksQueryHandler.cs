using AutoMapper;
using MediatR;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Application.Tasks.Queries.GetTasks;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, PagedResponseDto<TaskItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTasksQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponseDto<TaskItemDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var predicate = TaskFilterExpressionBuilder.Build(request.UserId, request.Filter);

        var (tasks, totalCount) = await _unitOfWork.Tasks.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            predicate,
            t => t.CreatedAt,
            cancellationToken);

        var taskDtos = _mapper.Map<List<TaskItemDto>>(tasks);

        var pagination = new PaginationDto
        {
            Page = request.PageNumber,
            Limit = request.PageSize,
            Total = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            HasPrevious = request.PageNumber > 1,
            HasNext = request.PageNumber * request.PageSize < totalCount
        };

        return new PagedResponseDto<TaskItemDto>
        {
            Data = taskDtos,
            Pagination = pagination,
            Success = true
        };
    }
}
