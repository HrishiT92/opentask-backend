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
public class AnalyticsController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IProjectService _projectService;

    public AnalyticsController(IApplicationDbContext context, IProjectService projectService)
    {
        _context = context;
        _projectService = projectService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
    }

    [HttpGet("project/{projectId}/dashboard")]
    public async Task<IActionResult> GetProjectDashboard(Guid projectId)
    {
        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(projectId, userId))
            return Forbid();

        var totalIssues = await _context.Issues.CountAsync(i => i.ProjectId == projectId);
        var openIssues = await _context.Issues.CountAsync(i => i.ProjectId == projectId && i.Status != IssueStatus.Done);
        var closedIssues = totalIssues - openIssues;

        var issuesByStatus = await _context.Issues
            .Where(i => i.ProjectId == projectId)
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var issuesByType = await _context.Issues
            .Where(i => i.ProjectId == projectId)
            .GroupBy(i => i.Type)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var issuesByPriority = await _context.Issues
            .Where(i => i.ProjectId == projectId)
            .GroupBy(i => i.Priority)
            .Select(g => new { Priority = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var issuesByAssignee = await _context.Issues
            .Where(i => i.ProjectId == projectId && i.AssigneeId != null)
            .Include(i => i.Assignee)
            .GroupBy(i => new { i.AssigneeId, i.Assignee!.FirstName, i.Assignee.LastName })
            .Select(g => new { 
                AssigneeId = g.Key.AssigneeId, 
                AssigneeName = $"{g.Key.FirstName} {g.Key.LastName}",
                Count = g.Count() 
            })
            .ToListAsync();

        return Ok(new
        {
            TotalIssues = totalIssues,
            OpenIssues = openIssues,
            ClosedIssues = closedIssues,
            IssuesByStatus = issuesByStatus,
            IssuesByType = issuesByType,
            IssuesByPriority = issuesByPriority,
            IssuesByAssignee = issuesByAssignee
        });
    }

    [HttpGet("project/{projectId}/velocity")]
    public async Task<IActionResult> GetProjectVelocity(Guid projectId)
    {
        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(projectId, userId))
            return Forbid();

        var completedSprints = await _context.Sprints
            .Where(s => s.ProjectId == projectId && s.Status == SprintStatus.Completed)
            .Include(s => s.Issues)
            .OrderBy(s => s.EndDate)
            .ToListAsync();

        var velocityData = completedSprints.Select(s => new
        {
            SprintName = s.Name,
            EndDate = s.EndDate.ToString("yyyy-MM-dd"),
            Velocity = s.Issues.Where(i => i.Status == IssueStatus.Done).Sum(i => i.StoryPoints)
        }).ToList();

        var averageVelocity = velocityData.Any() ? velocityData.Average(v => v.Velocity) : 0;

        return Ok(new
        {
            VelocityData = velocityData,
            AverageVelocity = averageVelocity
        });
    }

    [HttpGet("project/{projectId}/activity")]
    public async Task<IActionResult> GetProjectActivity(Guid projectId, [FromQuery] int days = 30)
    {
        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(projectId, userId))
            return Forbid();

        var startDate = DateTime.UtcNow.AddDays(-days);
        
        var activityData = await _context.ActivityLogs
            .Where(al => al.ProjectId == projectId && al.CreatedAt >= startDate)
            .Include(al => al.User)
            .OrderByDescending(al => al.CreatedAt)
            .Take(100)
            .Select(al => new
            {
                al.Id,
                al.Action,
                al.EntityType,
                al.EntityId,
                al.CreatedAt,
                User = new { al.User.FirstName, al.User.LastName }
            })
            .ToListAsync();

        return Ok(activityData);
    }
}
