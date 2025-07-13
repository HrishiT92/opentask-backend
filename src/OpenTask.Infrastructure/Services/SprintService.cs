using Microsoft.EntityFrameworkCore;
using OpenTask.Application.Interfaces;
using OpenTask.Application.Services;
using OpenTask.Domain.Entities;

namespace OpenTask.Infrastructure.Services;

public class SprintService : ISprintService
{
    private readonly IApplicationDbContext _context;

    public SprintService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Sprint>> GetProjectSprintsAsync(Guid projectId)
    {
        return await _context.Sprints
            .Include(s => s.Issues)
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Sprint?> GetSprintByIdAsync(Guid sprintId)
    {
        return await _context.Sprints
            .Include(s => s.Issues)
            .ThenInclude(i => i.Assignee)
            .FirstOrDefaultAsync(s => s.Id == sprintId);
    }

    public async Task<Sprint> CreateSprintAsync(Sprint sprint)
    {
        _context.Sprints.Add(sprint);
        await _context.SaveChangesAsync();
        return sprint;
    }

    public async Task<Sprint> UpdateSprintAsync(Sprint sprint)
    {
        _context.Sprints.Update(sprint);
        await _context.SaveChangesAsync();
        return sprint;
    }

    public async Task<bool> DeleteSprintAsync(Guid sprintId)
    {
        var sprint = await _context.Sprints.FindAsync(sprintId);
        if (sprint == null) return false;

        var issues = await _context.Issues.Where(i => i.SprintId == sprintId).ToListAsync();
        foreach (var issue in issues)
        {
            issue.SprintId = null;
        }

        _context.Sprints.Remove(sprint);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> StartSprintAsync(Guid sprintId)
    {
        var sprint = await _context.Sprints.FindAsync(sprintId);
        if (sprint == null || sprint.Status != SprintStatus.Planning) return false;

        sprint.Status = SprintStatus.Active;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteSprintAsync(Guid sprintId)
    {
        var sprint = await _context.Sprints.FindAsync(sprintId);
        if (sprint == null || sprint.Status != SprintStatus.Active) return false;

        sprint.Status = SprintStatus.Completed;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Sprint?> GetActiveSprintAsync(Guid projectId)
    {
        return await _context.Sprints
            .Include(s => s.Issues)
            .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.Status == SprintStatus.Active);
    }

    public async Task<int> GetSprintVelocityAsync(Guid sprintId)
    {
        return await _context.Issues
            .Where(i => i.SprintId == sprintId && i.Status == IssueStatus.Done)
            .SumAsync(i => i.StoryPoints);
    }

    public async Task<IEnumerable<object>> GetBurndownDataAsync(Guid sprintId)
    {
        var sprint = await _context.Sprints
            .Include(s => s.Issues)
            .FirstOrDefaultAsync(s => s.Id == sprintId);

        if (sprint == null) return new List<object>();

        var totalStoryPoints = sprint.Issues.Sum(i => i.StoryPoints);
        var sprintDays = (sprint.EndDate - sprint.StartDate).Days;
        
        var burndownData = new List<object>();
        
        for (int day = 0; day <= sprintDays; day++)
        {
            var currentDate = sprint.StartDate.AddDays(day);
            var completedPoints = sprint.Issues
                .Where(i => i.Status == IssueStatus.Done && i.UpdatedAt.Date <= currentDate.Date)
                .Sum(i => i.StoryPoints);
            
            var remainingPoints = totalStoryPoints - completedPoints;
            var idealRemaining = totalStoryPoints - (totalStoryPoints * day / sprintDays);
            
            burndownData.Add(new
            {
                Day = day,
                Date = currentDate.ToString("yyyy-MM-dd"),
                Remaining = remainingPoints,
                Ideal = idealRemaining
            });
        }

        return burndownData;
    }
}
