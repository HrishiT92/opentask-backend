using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenTask.Application.Services;
using OpenTask.Domain.Entities;
using System.Security.Claims;

namespace OpenTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly IProjectService _projectService;

    public CommentsController(ICommentService commentService, IProjectService projectService)
    {
        _commentService = commentService;
        _projectService = projectService;
    }

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
    }

    [HttpGet("issue/{issueId}")]
    public async Task<IActionResult> GetIssueComments(Guid issueId)
    {
        var userId = GetCurrentUserId();
        var comments = await _commentService.GetIssueCommentsAsync(issueId);
        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
    {
        var userId = GetCurrentUserId();
        
        var comment = new Comment
        {
            Content = request.Content,
            IssueId = request.IssueId,
            AuthorId = userId,
            ParentCommentId = request.ParentCommentId
        };

        var createdComment = await _commentService.CreateCommentAsync(comment);
        return CreatedAtAction(nameof(GetComment), new { id = createdComment.Id }, createdComment);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetComment(Guid id)
    {
        var comment = await _commentService.GetCommentByIdAsync(id);
        if (comment == null)
            return NotFound();

        return Ok(comment);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentRequest request)
    {
        var userId = GetCurrentUserId();
        var comment = await _commentService.GetCommentByIdAsync(id);
        
        if (comment == null)
            return NotFound();

        if (comment.AuthorId != userId)
            return Forbid();

        comment.Content = request.Content;
        var updatedComment = await _commentService.UpdateCommentAsync(comment);
        return Ok(updatedComment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var userId = GetCurrentUserId();
        var comment = await _commentService.GetCommentByIdAsync(id);
        
        if (comment == null)
            return NotFound();

        if (comment.AuthorId != userId)
            return Forbid();

        var result = await _commentService.DeleteCommentAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}

public class CreateCommentRequest
{
    public string Content { get; set; } = string.Empty;
    public Guid IssueId { get; set; }
    public Guid? ParentCommentId { get; set; }
}

public class UpdateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}
