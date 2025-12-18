using AutoMapper;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Entities;

namespace TaskMgr.Api.Application.Mappings;

/// <summary>
/// AutoMapper profile for entity to DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// Initializes mapping configurations
    /// </summary>
    public MappingProfile()
    {
        CreateUserMappings();
        CreateProjectMappings();
        CreateTaskMappings();
    }

    /// <summary>
    /// Creates user-related mappings
    /// </summary>
    private void CreateUserMappings()
    {
        CreateMap<User, UserDto>()
            .ReverseMap();

        CreateMap<UpdateUserProfileDto, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }

    /// <summary>
    /// Creates project-related mappings
    /// </summary>
    private void CreateProjectMappings()
    {
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.TotalTasksCount, opt => opt.MapFrom(src => src.Tasks.Count))
            .ForMember(dest => dest.CompletedTasksCount, opt => opt.MapFrom(src => src.Tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Done)))
            .ForMember(dest => dest.CompletionPercentage, opt => opt.MapFrom(src => src.GetCompletionPercentage()))
            .ReverseMap();

        CreateMap<CreateProjectDto, Project>()
            .ConstructUsing((src, context) => new Project(src.Name, Guid.Empty, src.Description))
            .ForMember(dest => dest.OwnerUserId, opt => opt.Ignore()) // Will be set by service
            .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color))
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate));

        CreateMap<UpdateProjectDto, Project>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }

    /// <summary>
    /// Creates task-related mappings
    /// </summary>
    private void CreateTaskMappings()
    {
        CreateMap<TaskItem, TaskItemDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.GetTagsList()))
            .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => src.IsOverdue()))
            .ForMember(dest => dest.IsDueSoon, opt => opt.MapFrom(src => src.IsDueSoon()))
            .ReverseMap()
            .ForMember(dest => dest.Tags, opt => opt.Ignore()); // Tags are set via SetTags method

        CreateMap<CreateTaskDto, TaskItem>()
            .ConstructUsing((src, context) => new TaskItem(src.Title, src.ProjectId, src.Priority, src.Description))
            .ForMember(dest => dest.AssignedToUserId, opt => opt.MapFrom(src => src.AssignedToUserId))
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
            .ForMember(dest => dest.EstimatedHours, opt => opt.MapFrom(src => src.EstimatedHours))
            .AfterMap((src, dest, context) =>
            {
                if (src.Tags.Any())
                    dest.SetTags(src.Tags);
            });

        CreateMap<UpdateTaskDto, TaskItem>()
            .ForMember(dest => dest.Tags, opt => opt.Ignore()) // Will be handled in service
            .ForMember(dest => dest.Status, opt => opt.Ignore()) // Status updates handled separately
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
