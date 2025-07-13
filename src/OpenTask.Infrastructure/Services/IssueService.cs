using Microsoft.EntityFrameworkCore;
using OpenTask.Application.Interfaces;
using OpenTask.Application.Services;
using OpenTask.Domain.Entities;

namespace OpenTask.Infrastructure.Services;

public class IssueService : IIssueService
{
    private readonly IApplicationDbContext _context;

    public IssueService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Issue>> GetProjectIssuesAsync(Guid projectId)
    {
        return await _context.Issues
            .Include(i => i.Assignee)
            .Include(i => i.Sprint)
            .Include(i => i.Labels)
            .Where(i => i.ProjectId == projectId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<Issue?> GetIssueByIdAsync(Guid issueId)
    {
        return await _context.Issues
            .Include(i => i.Assignee)
            .Include(i => i.Sprint)
            .Include(i => i.Labels)
            .Include(i => i.Comments)
            .ThenInclude(c => c.Author)
            .Include(i => i.Attachments)
            .Include(i => i.SubIssues)
            .FirstOrDefaultAsync(i => i.Id == issueId);
    }

    public async Task<Issue> CreateIssueAsync(Issue issue)
    {
        _context.Issues.Add(issue);
        await _context.SaveChangesAsync();
        return issue;
    }

    public async Task<Issue> UpdateIssueAsync(Issue issue)
    {
        _context.Issues.Update(issue);
        await _context.SaveChangesAsync();
        return issue;
    }

    public async Task<bool> DeleteIssueAsync(Guid issueId)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        if (issue == null) return false;

        _context.Issues.Remove(issue);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignIssueAsync(Guid issueId, Guid? assigneeId)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        if (issue == null) return false;

        issue.AssigneeId = assigneeId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateIssueStatusAsync(Guid issueId, IssueStatus status)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        if (issue == null) return false;

        issue.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Issue>> GetSprintIssuesAsync(Guid sprintId)
    {
        return await _context.Issues
            .Include(i => i.Assignee)
            .Include(i => i.Labels)
            .Where(i => i.SprintId == sprintId)
            .OrderBy(i => i.Status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Issue>> GetBacklogIssuesAsync(Guid projectId)
    {
        return await _context.Issues
            .Include(i => i.Assignee)
            .Include(i => i.Labels)
            .Where(i => i.ProjectId == projectId && i.SprintId == null)
            .OrderByDescending(i => i.Priority)
            .ThenByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> MoveIssueToSprintAsync(Guid issueId, Guid? sprintId)
    {
        var issue = await _context.Issues.FindAsync(issueId);
        if (issue == null) return false;

        issue.SprintId = sprintId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Issue>> SearchIssuesAsync(string query, Guid projectId)
    {
        return await _context.Issues
            .Include(i => i.Assignee)
            .Include(i => i.Labels)
            .Where(i => i.ProjectId == projectId && 
                       (i.Title.Contains(query) || 
                        (i.Description != null && i.Description.Contains(query))))
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Issue>> GetIssuesByFilterAsync(Guid projectId, IssueStatus? status, Guid? assigneeId, Priority? priority)
    {
        var query = _context.Issues
            .Include(i => i.Assignee)
            .Include(i => i.Labels)
            .Where(i => i.ProjectId == projectId);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        if (assigneeId.HasValue)
            query = query.Where(i => i.AssigneeId == assigneeId.Value);

        if (priority.HasValue)
            query = query.Where(i => i.Priority == priority.Value);

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }
}
