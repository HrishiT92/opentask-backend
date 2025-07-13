using System.ComponentModel.DataAnnotations;

namespace OpenTask.Domain.Entities;

public class Sprint : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(500)]
    public string? Goal { get; set; }
    
    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; } = null!;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public SprintStatus Status { get; set; } = SprintStatus.Planning;
    
    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();
}

public enum SprintStatus
{
    Planning = 1,
    Active = 2,
    Completed = 3,
    Cancelled = 4
}
