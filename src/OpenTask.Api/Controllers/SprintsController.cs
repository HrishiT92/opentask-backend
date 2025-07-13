using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenTask.Application.Services;
using OpenTask.Domain.Entities;
using System.Security.Claims;

namespace OpenTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SprintsController : ControllerBase
{
    private readonly ISprintService _sprintService;
    private readonly IProjectService _projectService;

    public SprintsController(ISprintService sprintService, IProjectService projectService)
    {
        _sprintService = sprintService;
        _projectService = projectService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetProjectSprints(Guid projectId)
    {
        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(projectId, userId))
            return Forbid();

        var sprints = await _sprintService.GetProjectSprintsAsync(projectId);
        return Ok(sprints);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSprint(Guid id)
    {
        var sprint = await _sprintService.GetSprintByIdAsync(id);
        if (sprint == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(sprint.ProjectId, userId))
            return Forbid();

        return Ok(sprint);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSprint([FromBody] CreateSprintRequest request)
    {
        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(request.ProjectId, userId))
            return Forbid();

        var sprint = new Sprint
        {
            Name = request.Name,
            Goal = request.Goal,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ProjectId = request.ProjectId,
            Status = SprintStatus.Planning
        };

        var createdSprint = await _sprintService.CreateSprintAsync(sprint);
        return CreatedAtAction(nameof(GetSprint), new { id = createdSprint.Id }, createdSprint);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSprint(Guid id, [FromBody] UpdateSprintRequest request)
    {
        var sprint = await _sprintService.GetSprintByIdAsync(id);
        if (sprint == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(sprint.ProjectId, userId))
            return Forbid();

        sprint.Name = request.Name;
        sprint.Goal = request.Goal;
        sprint.StartDate = request.StartDate;
        sprint.EndDate = request.EndDate;

        var updatedSprint = await _sprintService.UpdateSprintAsync(sprint);
        return Ok(updatedSprint);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSprint(Guid id)
    {
        var sprint = await _sprintService.GetSprintByIdAsync(id);
        if (sprint == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(sprint.ProjectId, userId))
            return Forbid();

        var result = await _sprintService.DeleteSprintAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartSprint(Guid id)
    {
        var sprint = await _sprintService.GetSprintByIdAsync(id);
        if (sprint == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(sprint.ProjectId, userId))
            return Forbid();

        var result = await _sprintService.StartSprintAsync(id);
        if (!result)
            return BadRequest("Sprint cannot be started");

        return Ok();
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteSprint(Guid id)
    {
        var sprint = await _sprintService.GetSprintByIdAsync(id);
        if (sprint == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(sprint.ProjectId, userId))
            return Forbid();

        var result = await _sprintService.CompleteSprintAsync(id);
        if (!result)
            return BadRequest("Sprint cannot be completed");

        return Ok();
    }

    [HttpGet("project/{projectId}/active")]
    public async Task<IActionResult> GetActiveSprint(Guid projectId)
    {
        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(projectId, userId))
            return Forbid();

        var sprint = await _sprintService.GetActiveSprintAsync(projectId);
        return Ok(sprint);
    }

    [HttpGet("{id}/velocity")]
    public async Task<IActionResult> GetSprintVelocity(Guid id)
    {
        var sprint = await _sprintService.GetSprintByIdAsync(id);
        if (sprint == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(sprint.ProjectId, userId))
            return Forbid();

        var velocity = await _sprintService.GetSprintVelocityAsync(id);
        return Ok(new { velocity });
    }

    [HttpGet("{id}/burndown")]
    public async Task<IActionResult> GetBurndownData(Guid id)
    {
        var sprint = await _sprintService.GetSprintByIdAsync(id);
        if (sprint == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(sprint.ProjectId, userId))
            return Forbid();

        var burndownData = await _sprintService.GetBurndownDataAsync(id);
        return Ok(burndownData);
    }
}

public class CreateSprintRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid ProjectId { get; set; }
}

public class UpdateSprintRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
