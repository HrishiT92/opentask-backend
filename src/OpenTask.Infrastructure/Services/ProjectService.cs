using Microsoft.EntityFrameworkCore;
using OpenTask.Application.Interfaces;
using OpenTask.Application.Services;
using OpenTask.Domain.Entities;

namespace OpenTask.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly IApplicationDbContext _context;

    public ProjectService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetUserProjectsAsync(Guid userId)
    {
        return await _context.ProjectMembers
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.Project)
            .ToListAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(Guid projectId)
    {
        return await _context.Projects
            .Include(p => p.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == projectId);
    }

    public async Task<Project> CreateProjectAsync(Project project, Guid userId)
    {
        _context.Projects.Add(project);
        
        var membership = new ProjectMember
        {
            UserId = userId,
            ProjectId = project.Id,
            Role = ProjectRole.Owner
        };
        
        _context.ProjectMembers.Add(membership);
        await _context.SaveChangesAsync();
        
        return project;
    }

    public async Task<Project> UpdateProjectAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<bool> DeleteProjectAsync(Guid projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null) return false;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddMemberAsync(Guid projectId, Guid userId, ProjectRole role)
    {
        var existingMember = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        
        if (existingMember != null) return false;

        var membership = new ProjectMember
        {
            UserId = userId,
            ProjectId = projectId,
            Role = role
        };

        _context.ProjectMembers.Add(membership);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveMemberAsync(Guid projectId, Guid userId)
    {
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        
        if (member == null) return false;

        _context.ProjectMembers.Remove(member);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateMemberRoleAsync(Guid projectId, Guid userId, ProjectRole role)
    {
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        
        if (member == null) return false;

        member.Role = role;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(Guid projectId)
    {
        return await _context.ProjectMembers
            .Include(pm => pm.User)
            .Where(pm => pm.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<bool> HasAccessAsync(Guid projectId, Guid userId)
    {
        return await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
    }

    public async Task<ProjectRole?> GetUserRoleAsync(Guid projectId, Guid userId)
    {
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        
        return member?.Role;
    }
}
