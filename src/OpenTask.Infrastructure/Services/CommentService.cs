using Microsoft.EntityFrameworkCore;
using OpenTask.Application.Interfaces;
using OpenTask.Application.Services;
using OpenTask.Domain.Entities;

namespace OpenTask.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly IApplicationDbContext _context;

    public CommentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Comment>> GetIssueCommentsAsync(Guid issueId)
    {
        return await _context.Comments
            .Include(c => c.Author)
            .Include(c => c.Replies)
            .ThenInclude(r => r.Author)
            .Where(c => c.IssueId == issueId && c.ParentCommentId == null)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Comment?> GetCommentByIdAsync(Guid commentId)
    {
        return await _context.Comments
            .Include(c => c.Author)
            .Include(c => c.Replies)
            .ThenInclude(r => r.Author)
            .FirstOrDefaultAsync(c => c.Id == commentId);
    }

    public async Task<Comment> CreateCommentAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        
        return await GetCommentByIdAsync(comment.Id) ?? comment;
    }

    public async Task<Comment> UpdateCommentAsync(Comment comment)
    {
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null) return false;

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }
}
