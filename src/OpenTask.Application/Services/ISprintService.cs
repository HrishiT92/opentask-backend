using OpenTask.Domain.Entities;

namespace OpenTask.Application.Services;

public interface ISprintService
{
    Task<IEnumerable<Sprint>> GetProjectSprintsAsync(Guid projectId);
    Task<Sprint?> GetSprintByIdAsync(Guid sprintId);
    Task<Sprint> CreateSprintAsync(Sprint sprint);
    Task<Sprint> UpdateSprintAsync(Sprint sprint);
    Task<bool> DeleteSprintAsync(Guid sprintId);
    Task<bool> StartSprintAsync(Guid sprintId);
    Task<bool> CompleteSprintAsync(Guid sprintId);
    Task<Sprint?> GetActiveSprintAsync(Guid projectId);
    Task<int> GetSprintVelocityAsync(Guid sprintId);
    Task<IEnumerable<object>> GetBurndownDataAsync(Guid sprintId);
    Task<object> CalculateVelocityAsync(Guid sprintId);
}
