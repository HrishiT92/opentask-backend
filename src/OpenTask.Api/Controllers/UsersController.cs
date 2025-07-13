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
public class UsersController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthService _authService;

    public UsersController(IApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
            return NotFound();

        return Ok(new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            user.IsActive
        });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest request)
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
            return NotFound();

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        
        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            if (string.IsNullOrEmpty(request.CurrentPassword) || 
                !_authService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest("Current password is incorrect");
            }
            
            user.PasswordHash = _authService.HashPassword(request.NewPassword);
        }

        await _context.SaveChangesAsync();
        
        return Ok(new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            user.IsActive
        });
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetProjectUsers(Guid projectId)
    {
        var userId = GetCurrentUserId();
        var projectService = HttpContext.RequestServices.GetRequiredService<IProjectService>();
        
        if (!await projectService.HasAccessAsync(projectId, userId))
            return Forbid();

        var users = await _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .Include(pm => pm.User)
            .Select(pm => new
            {
                pm.User.Id,
                pm.User.FirstName,
                pm.User.LastName,
                pm.User.Email,
                pm.Role
            })
            .ToListAsync();

        return Ok(users);
    }
}

public class UpdateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}
