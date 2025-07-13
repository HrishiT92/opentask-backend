using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenTask.Application.Interfaces;
using OpenTask.Application.Services;
using OpenTask.Domain.Entities;
using System.Security.Claims;

namespace OpenTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IApplicationDbContext _context;
    private readonly IProjectService _projectService;

    public AttachmentsController(
        IFileStorageService fileStorageService, 
        IApplicationDbContext context,
        IProjectService projectService)
    {
        _fileStorageService = fileStorageService;
        _context = context;
        _projectService = projectService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
    }

    [HttpPost("issue/{issueId}")]
    public async Task<IActionResult> UploadAttachment(Guid issueId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        var issue = await _context.Issues.FindAsync(issueId);
        if (issue == null)
            return NotFound("Issue not found");

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(issue.ProjectId, userId))
            return Forbid();

        try
        {
            var filePath = await _fileStorageService.UploadFileAsync(file.OpenReadStream(), file.FileName, file.ContentType);
            
            var attachment = new IssueAttachment
            {
                FileName = file.FileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                IssueId = issueId,
                UploadedById = userId
            };

            _context.IssueAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return Ok(attachment);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> DownloadAttachment(Guid id)
    {
        var attachment = await _context.IssueAttachments.FindAsync(id);
        if (attachment == null)
            return NotFound();

        var issue = await _context.Issues.FindAsync(attachment.IssueId);
        if (issue == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(issue.ProjectId, userId))
            return Forbid();

        try
        {
            var fileStream = await _fileStorageService.DownloadFileAsync(attachment.FilePath);
            return File(fileStream, attachment.ContentType, attachment.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound("File not found");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAttachment(Guid id)
    {
        var attachment = await _context.IssueAttachments.FindAsync(id);
        if (attachment == null)
            return NotFound();

        var issue = await _context.Issues.FindAsync(attachment.IssueId);
        if (issue == null)
            return NotFound();

        var userId = GetCurrentUserId();
        if (!await _projectService.HasAccessAsync(issue.ProjectId, userId))
            return Forbid();

        await _fileStorageService.DeleteFileAsync(attachment.FilePath);
        _context.IssueAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
