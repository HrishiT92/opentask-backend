using OpenTask.Domain.Entities;

namespace OpenTask.Application.Services;

public interface IProjectService
{
    Task<IEnumerable<Project>> GetUserProjectsAsync(Guid userId);
    Task<Project?> GetProjectByIdAsync(Guid projectId);
    Task<Project> CreateProjectAsync(Project project, Guid userId);
    Task<Project> UpdateProjectAsync(Project project);
    Task<bool> DeleteProjectAsync(Guid projectId);
    Task<bool> AddMemberAsync(Guid projectId, Guid userId, ProjectRole role);
    Task<bool> RemoveMemberAsync(Guid projectId, Guid userId);
    Task<bool> UpdateMemberRoleAsync(Guid projectId, Guid userId, ProjectRole role);
    Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(Guid projectId);
    Task<bool> HasAccessAsync(Guid projectId, Guid userId);
    Task<ProjectRole?> GetUserRoleAsync(Guid projectId, Guid userId);
}
