using AutoMapper;
using Microsoft.Extensions.Logging;
using TaskMgr.Api.Application.DTOs;
using TaskMgr.Api.Domain.Entities;
using TaskMgr.Api.Domain.Interfaces;

namespace TaskMgr.Api.Application.Services;

/// <summary>
/// Service for project-related operations
/// </summary>
public class ProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProjectService> _logger;

    /// <summary>
    /// Initializes a new instance of the ProjectService
    /// </summary>
    /// <param name="unitOfWork">Unit of work</param>
    /// <param name="mapper">AutoMapper instance</param>
    /// <param name="logger">Logger instance</param>
    public ProjectService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProjectService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all projects for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="includeArchived">Whether to include archived projects</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of project DTOs</returns>
    public async Task<List<ProjectDto>> GetUserProjectsAsync(Guid userId, bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting projects for user: {UserId}, includeArchived: {IncludeArchived}", userId, includeArchived);

        var projects = await _unitOfWork.Projects.FindAsync(
            p => p.OwnerUserId == userId && (includeArchived || !p.IsArchived),
            cancellationToken);

        var projectDtos = _mapper.Map<List<ProjectDto>>(projects);
        
        _logger.LogDebug("Found {ProjectCount} projects for user: {UserId}", projectDtos.Count, userId);
        return projectDtos;
    }

    /// <summary>
    /// Gets a project by ID
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="userId">User ID (for ownership verification)</param>
    /// <param name="includeTasks">Whether to include tasks</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Project DTO or null if not found</returns>
    public async Task<ProjectDto?> GetProjectByIdAsync(Guid projectId, Guid userId, bool includeTasks = false, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting project {ProjectId} for user {UserId}", projectId, userId);

        var project = await _unitOfWork.Projects.GetSingleAsync(
            p => p.Id == projectId && p.OwnerUserId == userId,
            cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project {ProjectId} not found or not owned by user {UserId}", projectId, userId);
            return null;
        }

        if (includeTasks)
        {
            var tasks = await _unitOfWork.Tasks.FindAsync(t => t.ProjectId == projectId, cancellationToken);
            project.Tasks.Clear();
            foreach (var task in tasks)
            {
                project.Tasks.Add(task);
            }
        }

        return _mapper.Map<ProjectDto>(project);
    }

    /// <summary>
    /// Creates a new project
    /// </summary>
    /// <param name="createProjectDto">Project creation data</param>
    /// <param name="userId">Owner user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created project DTO</returns>
    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto createProjectDto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new project '{ProjectName}' for user {UserId}", createProjectDto.Name, userId);

        // Verify user exists
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("User {UserId} not found or inactive", userId);
            throw new InvalidOperationException($"User with ID {userId} not found or inactive");
        }

        var project = new Project(createProjectDto.Name, userId, createProjectDto.Description);
        
        if (!string.IsNullOrEmpty(createProjectDto.Color))
            project.Update(color: createProjectDto.Color);
        
        if (createProjectDto.DueDate.HasValue)
            project.Update(dueDate: createProjectDto.DueDate);

        await _unitOfWork.Projects.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created project {ProjectId} for user {UserId}", project.Id, userId);
        return _mapper.Map<ProjectDto>(project);
    }

    /// <summary>
    /// Updates an existing project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="updateProjectDto">Project update data</param>
    /// <param name="userId">Owner user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated project DTO</returns>
    public async Task<ProjectDto?> UpdateProjectAsync(Guid projectId, UpdateProjectDto updateProjectDto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating project {ProjectId} for user {UserId}", projectId, userId);

        var project = await _unitOfWork.Projects.GetSingleAsync(
            p => p.Id == projectId && p.OwnerUserId == userId,
            cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project {ProjectId} not found or not owned by user {UserId}", projectId, userId);
            return null;
        }

        project.Update(
            updateProjectDto.Name,
            updateProjectDto.Description,
            updateProjectDto.Color,
            updateProjectDto.DueDate);

        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated project {ProjectId}", projectId);
        return _mapper.Map<ProjectDto>(project);
    }

    /// <summary>
    /// Deletes a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="userId">Owner user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteProjectAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting project {ProjectId} for user {UserId}", projectId, userId);

        var project = await _unitOfWork.Projects.GetSingleAsync(
            p => p.Id == projectId && p.OwnerUserId == userId,
            cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project {ProjectId} not found or not owned by user {UserId}", projectId, userId);
            return false;
        }

        // Check if project has tasks
        var hasActiveTasks = await _unitOfWork.Tasks.AnyAsync(
            t => t.ProjectId == projectId && t.Status != Domain.Enums.TaskStatus.Done,
            cancellationToken);

        if (hasActiveTasks)
        {
            _logger.LogWarning("Cannot delete project {ProjectId} - has active tasks", projectId);
            throw new InvalidOperationException("Cannot delete project with active tasks. Complete or delete all tasks first.");
        }

        _unitOfWork.Projects.Remove(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted project {ProjectId}", projectId);
        return true;
    }

    /// <summary>
    /// Archives a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="userId">Owner user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if archived successfully</returns>
    public async Task<bool> ArchiveProjectAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Archiving project {ProjectId} for user {UserId}", projectId, userId);

        var project = await _unitOfWork.Projects.GetSingleAsync(
            p => p.Id == projectId && p.OwnerUserId == userId,
            cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project {ProjectId} not found or not owned by user {UserId}", projectId, userId);
            return false;
        }

        project.Archive();
        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully archived project {ProjectId}", projectId);
        return true;
    }

    /// <summary>
    /// Unarchives a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="userId">Owner user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if unarchived successfully</returns>
    public async Task<bool> UnarchiveProjectAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Unarchiving project {ProjectId} for user {UserId}", projectId, userId);

        var project = await _unitOfWork.Projects.GetSingleAsync(
            p => p.Id == projectId && p.OwnerUserId == userId,
            cancellationToken);

        if (project == null)
        {
            _logger.LogWarning("Project {ProjectId} not found or not owned by user {UserId}", projectId, userId);
            return false;
        }

        project.Unarchive();
        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully unarchived project {ProjectId}", projectId);
        return true;
    }
}
