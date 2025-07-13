using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenTask.Application.Services;
using OpenTask.Domain.Entities;
using System.Security.Claims;

namespace OpenTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IssuesController : ControllerBase
{
    private readonly IIssueService _issueService;
    private readonly IProjectService _projectService;

    public IssuesController(IIssueService issueService, IProjectService projectService)
    {
        _issueService = issueService;
        _projectService = projectService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetProjectIssues(Guid projectId)
    {
        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(projectId, userId))
            return Forbid();

        var issues = await _issueService.GetProjectIssuesAsync(projectId);
        return Ok(issues);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetIssue(Guid id)
    {
        var issue = await _issueService.GetIssueByIdAsync(id);
        if (issue == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(issue.ProjectId, userId))
            return Forbid();

        return Ok(issue);
    }

    [HttpPost]
    public async Task<IActionResult> CreateIssue([FromBody] CreateIssueRequest request)
    {
        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(request.ProjectId, userId))
            return Forbid();

        var issue = new Issue
        {
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            Priority = request.Priority,
            ProjectId = request.ProjectId,
            AssigneeId = request.AssigneeId,
            SprintId = request.SprintId,
            StoryPoints = request.StoryPoints,
            DueDate = request.DueDate
        };

        var createdIssue = await _issueService.CreateIssueAsync(issue);
        return CreatedAtAction(nameof(GetIssue), new { id = createdIssue.Id }, createdIssue);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateIssue(Guid id, [FromBody] UpdateIssueRequest request)
    {
        var issue = await _issueService.GetIssueByIdAsync(id);
        if (issue == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(issue.ProjectId, userId))
            return Forbid();

        issue.Title = request.Title;
        issue.Description = request.Description;
        issue.Type = request.Type;
        issue.Priority = request.Priority;
        issue.Status = request.Status;
        issue.AssigneeId = request.AssigneeId;
        issue.StoryPoints = request.StoryPoints;
        issue.DueDate = request.DueDate;

        var updatedIssue = await _issueService.UpdateIssueAsync(issue);
        return Ok(updatedIssue);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIssue(Guid id)
    {
        var issue = await _issueService.GetIssueByIdAsync(id);
        if (issue == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(issue.ProjectId, userId))
            return Forbid();

        var result = await _issueService.DeleteIssueAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateIssueStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var issue = await _issueService.GetIssueByIdAsync(id);
        if (issue == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(issue.ProjectId, userId))
            return Forbid();

        var result = await _issueService.UpdateIssueStatusAsync(id, request.Status);
        if (!result)
            return NotFound();

        return Ok();
    }

    [HttpPut("{id}/assign")]
    public async Task<IActionResult> AssignIssue(Guid id, [FromBody] AssignIssueRequest request)
    {
        var issue = await _issueService.GetIssueByIdAsync(id);
        if (issue == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(issue.ProjectId, userId))
            return Forbid();

        var result = await _issueService.AssignIssueAsync(id, request.AssigneeId);
        if (!result)
            return NotFound();

        return Ok();
    }

    [HttpGet("project/{projectId}/backlog")]
    public async Task<IActionResult> GetBacklogIssues(Guid projectId)
    {
        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(projectId, userId))
            return Forbid();

        var issues = await _issueService.GetBacklogIssuesAsync(projectId);
        return Ok(issues);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchIssues([FromQuery] string query, [FromQuery] Guid projectId)
    {
        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(projectId, userId))
            return Forbid();

        var issues = await _issueService.SearchIssuesAsync(query, projectId);
        return Ok(issues);
    }

    [HttpGet("filter")]
    public async Task<IActionResult> FilterIssues(
        [FromQuery] Guid projectId,
        [FromQuery] IssueStatus? status,
        [FromQuery] Guid? assigneeId,
        [FromQuery] Priority? priority)
    {
        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(projectId, userId))
            return Forbid();

        var issues = await _issueService.GetIssuesByFilterAsync(projectId, status, assigneeId, priority);
        return Ok(issues);
    }
}

public class CreateIssueRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IssueType Type { get; set; }
    public Priority Priority { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid? SprintId { get; set; }
    public int StoryPoints { get; set; }
    public DateTime? DueDate { get; set; }
}

public class UpdateIssueRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IssueType Type { get; set; }
    public Priority Priority { get; set; }
    public IssueStatus Status { get; set; }
    public Guid? AssigneeId { get; set; }
    public int StoryPoints { get; set; }
    public DateTime? DueDate { get; set; }
}

public class UpdateStatusRequest
{
    public IssueStatus Status { get; set; }
}

public class AssignIssueRequest
{
    public Guid? AssigneeId { get; set; }
}
