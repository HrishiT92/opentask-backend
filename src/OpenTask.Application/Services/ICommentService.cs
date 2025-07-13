using OpenTask.Domain.Entities;

namespace OpenTask.Application.Services;

public interface ICommentService
{
    Task<IEnumerable<Comment>> GetIssueCommentsAsync(Guid issueId);
    Task<Comment?> GetCommentByIdAsync(Guid commentId);
    Task<Comment> CreateCommentAsync(Comment comment);
    Task<Comment> UpdateCommentAsync(Comment comment);
    Task<bool> DeleteCommentAsync(Guid commentId);
}
