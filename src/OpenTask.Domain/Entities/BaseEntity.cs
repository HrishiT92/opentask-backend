using System.ComponentModel.DataAnnotations;

namespace OpenTask.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class User : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.Viewer;
    
    public bool IsActive { get; set; } = true;
    
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    public ICollection<Issue> AssignedIssues { get; set; } = new List<Issue>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

public enum UserRole
{
    Admin = 1,
    ProjectManager = 2,
    Developer = 3,
    QA = 4,
    Viewer = 5
}

public class Project : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Key { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Issue> Issues { get; set; } = new List<Issue>();
    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
}

public class ProjectMember : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public ProjectRole Role { get; set; } = ProjectRole.Developer;
}

public enum ProjectRole
{
    Owner = 1,
    Admin = 2,
    Developer = 3,
    QA = 4,
    Viewer = 5
}

public class Issue : BaseEntity
{
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public IssueType Type { get; set; } = IssueType.Task;
    
    public Priority Priority { get; set; } = Priority.Medium;
    
    public IssueStatus Status { get; set; } = IssueStatus.ToDo;
    
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public Guid? AssigneeId { get; set; }
    public User? Assignee { get; set; }
    
    public Guid? SprintId { get; set; }
    public Sprint? Sprint { get; set; }
    
    public Guid? ParentIssueId { get; set; }
    public Issue? ParentIssue { get; set; }
    
    public int StoryPoints { get; set; } = 0;
    
    public DateTime? DueDate { get; set; }
    
    public ICollection<Issue> SubIssues { get; set; } = new List<Issue>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<IssueAttachment> Attachments { get; set; } = new List<IssueAttachment>();
    public ICollection<IssueLabel> Labels { get; set; } = new List<IssueLabel>();
}

public enum IssueType
{
    Bug = 1,
    Task = 2,
    Story = 3,
    Epic = 4
}

public enum Priority
{
    Lowest = 1,
    Low = 2,
    Medium = 3,
    High = 4,
    Highest = 5
}

public enum IssueStatus
{
    ToDo = 1,
    InProgress = 2,
    InReview = 3,
    Done = 4,
    Cancelled = 5
}
