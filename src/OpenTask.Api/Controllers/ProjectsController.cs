using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenTask.Application.Services;
using OpenTask.Domain.Entities;
using System.Security.Claims;

namespace OpenTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
    }

    [HttpGet]
    public async Task<IActionResult> GetUserProjects()
    {
        var userId = GetCurrentUserId();
        var projects = await _projectService.GetUserProjectsAsync(userId);
        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProject(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(id, userId))
            return Forbid();

        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
            return NotFound();

        return Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        var userId = GetCurrentUserId();
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            Key = request.Key
        };

        var createdProject = await _projectService.CreateProjectAsync(project, userId);
        return CreatedAtAction(nameof(GetProject), new { id = createdProject.Id }, createdProject);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
    {
        var userId = GetCurrentUserId();
        var userRole = await _projectService.GetUserRoleAsync(id, userId);
        
        if (userRole == null || (userRole != ProjectRole.Owner && userRole != ProjectRole.Admin))
            return Forbid();

        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
            return NotFound();

        project.Name = request.Name;
        project.Description = request.Description;
        project.IsActive = request.IsActive;

        var updatedProject = await _projectService.UpdateProjectAsync(project);
        return Ok(updatedProject);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        var userId = GetCurrentUserId();
        var userRole = await _projectService.GetUserRoleAsync(id, userId);
        
        if (userRole != ProjectRole.Owner)
            return Forbid();

        var result = await _projectService.DeleteProjectAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetProjectMembers(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(id, userId))
            return Forbid();

        var members = await _projectService.GetProjectMembersAsync(id);
        return Ok(members);
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberRequest request)
    {
        var userId = GetCurrentUserId();
        var userRole = await _projectService.GetUserRoleAsync(id, userId);
        
        if (userRole == null || (userRole != ProjectRole.Owner && userRole != ProjectRole.Admin))
            return Forbid();

        var result = await _projectService.AddMemberAsync(id, request.UserId, request.Role);
        if (!result)
            return BadRequest("User is already a member or does not exist");

        return Ok();
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        var userRole = await _projectService.GetUserRoleAsync(id, currentUserId);
        
        if (userRole == null || (userRole != ProjectRole.Owner && userRole != ProjectRole.Admin))
            return Forbid();

        var result = await _projectService.RemoveMemberAsync(id, userId);
        if (!result)
            return NotFound();

        return NoContent();
    }
}

public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Key { get; set; } = string.Empty;
}

public class UpdateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AddMemberRequest
{
    public Guid UserId { get; set; }
    public ProjectRole Role { get; set; }
}
