using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTask.Application.Interfaces;
using OpenTask.Application.Services;
using OpenTask.Domain.Entities;
using System.Security.Claims;

namespace OpenTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IProjectService _projectService;

    public SearchController(IApplicationDbContext context, IProjectService projectService)
    {
        _context = context;
        _projectService = projectService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
    }

    [HttpGet("issues")]
    public async Task<IActionResult> SearchIssues(
        [FromQuery] string? query,
        [FromQuery] Guid? projectId,
        [FromQuery] IssueStatus? status,
        [FromQuery] Guid? assigneeId,
        [FromQuery] Priority? priority,
        [FromQuery] IssueType? type,
        [FromQuery] Guid? sprintId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        
        var issuesQuery = _context.Issues
            .Include(i => i.Assignee)
            .Include(i => i.Sprint)
            .Include(i => i.Labels)
            .Include(i => i.Project)
            .AsQueryable();

        if (projectId.HasValue)
        {
            if (!await _projectService.HasAccessAsync(projectId.Value, userId))
                return Forbid();
            
            issuesQuery = issuesQuery.Where(i => i.ProjectId == projectId.Value);
        }
        else
        {
            var userProjectIds = await _context.ProjectMembers
                .Where(pm => pm.UserId == userId)
                .Select(pm => pm.ProjectId)
                .ToListAsync();
            
            issuesQuery = issuesQuery.Where(i => userProjectIds.Contains(i.ProjectId));
        }

        if (!string.IsNullOrEmpty(query))
        {
            issuesQuery = issuesQuery.Where(i => 
                i.Title.Contains(query) || 
                (i.Description != null && i.Description.Contains(query)));
        }

        if (status.HasValue)
            issuesQuery = issuesQuery.Where(i => i.Status == status.Value);

        if (assigneeId.HasValue)
            issuesQuery = issuesQuery.Where(i => i.AssigneeId == assigneeId.Value);

        if (priority.HasValue)
            issuesQuery = issuesQuery.Where(i => i.Priority == priority.Value);

        if (type.HasValue)
            issuesQuery = issuesQuery.Where(i => i.Type == type.Value);

        if (sprintId.HasValue)
            issuesQuery = issuesQuery.Where(i => i.SprintId == sprintId.Value);

        var totalCount = await issuesQuery.CountAsync();
        
        var issues = await issuesQuery
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            Issues = issues,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpGet("projects")]
    public async Task<IActionResult> SearchProjects([FromQuery] string? query)
    {
        var userId = GetCurrentUserId();
        
        var projectsQuery = _context.ProjectMembers
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.Project)
            .AsQueryable();

        if (!string.IsNullOrEmpty(query))
        {
            projectsQuery = projectsQuery.Where(p => 
                p.Name.Contains(query) || 
                (p.Description != null && p.Description.Contains(query)));
        }

        var projects = await projectsQuery
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Ok(projects);
    }
}
