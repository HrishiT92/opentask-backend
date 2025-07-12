using System.ComponentModel.DataAnnotations;

namespace OpenTask.Domain.Entities;

public class Sprint : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public SprintStatus Status { get; set; } = SprintStatus.Planning;
    
    public ICollection<Issue> Issues { get; set; } = new List<Issue>();
}

public enum SprintStatus
{
    Planning = 1,
    Active = 2,
    Completed = 3,
    Cancelled = 4
}

public class Comment : BaseEntity
{
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public Guid IssueId { get; set; }
    public Issue Issue { get; set; } = null!;
    
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
    
    public Guid? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}

public class IssueAttachment : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    public Guid IssueId { get; set; }
    public Issue Issue { get; set; } = null!;
}

public class IssueLabel : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(7)]
    public string Color { get; set; } = "#000000";
    
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public ICollection<Issue> Issues { get; set; } = new List<Issue>();
}

public class ActivityLog : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    public Guid EntityId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;
    
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}
