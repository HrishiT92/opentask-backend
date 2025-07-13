using OpenTask.Domain.Entities;

namespace OpenTask.Application.Services;

public interface IIssueService
{
    Task<IEnumerable<Issue>> GetProjectIssuesAsync(Guid projectId);
    Task<Issue?> GetIssueByIdAsync(Guid issueId);
    Task<Issue> CreateIssueAsync(Issue issue);
    Task<Issue> UpdateIssueAsync(Issue issue);
    Task<bool> DeleteIssueAsync(Guid issueId);
    Task<bool> AssignIssueAsync(Guid issueId, Guid? assigneeId);
    Task<bool> UpdateIssueStatusAsync(Guid issueId, IssueStatus status);
    Task<IEnumerable<Issue>> GetSprintIssuesAsync(Guid sprintId);
    Task<IEnumerable<Issue>> GetBacklogIssuesAsync(Guid projectId);
    Task<bool> MoveIssueToSprintAsync(Guid issueId, Guid? sprintId);
    Task<IEnumerable<Issue>> SearchIssuesAsync(string query, Guid projectId);
    Task<IEnumerable<Issue>> GetIssuesByFilterAsync(Guid projectId, IssueStatus? status, Guid? assigneeId, Priority? priority);
    Task<IssueDependency> CreateDependencyAsync(Guid blockingIssueId, Guid blockedIssueId, string type);
    Task<IEnumerable<IssueDependency>> GetDependenciesAsync(Guid issueId);
    Task DeleteDependencyAsync(Guid dependencyId);
}
