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
    
    public virtual ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    public virtual ICollection<Issue> AssignedIssues { get; set; } = new List<Issue>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
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
    
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
}

public class ProjectMember : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid ProjectId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
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
    [System.Text.Json.Serialization.JsonIgnore]
    public Project Project { get; set; } = null!;
    
    public Guid? AssigneeId { get; set; }
    public User? Assignee { get; set; }
    
    public Guid? SprintId { get; set; }
    public Sprint? Sprint { get; set; }
    
    public Guid? ParentIssueId { get; set; }
    public Issue? ParentIssue { get; set; }
    
    public int StoryPoints { get; set; } = 0;
    
    public DateTime? DueDate { get; set; }
    
    public virtual ICollection<IssueAttachment> Attachments { get; set; } = new List<IssueAttachment>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<IssueLabel> Labels { get; set; } = new List<IssueLabel>();
    public virtual ICollection<Issue> SubIssues { get; set; } = new List<Issue>();
    public virtual ICollection<IssueDependency> BlockedByDependencies { get; set; } = new List<IssueDependency>();
    public virtual ICollection<IssueDependency> BlockingDependencies { get; set; } = new List<IssueDependency>();
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
    public virtual Issue Issue { get; set; } = null!;
    
    public Guid UploadedById { get; set; }
    public virtual User UploadedBy { get; set; } = null!;
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
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Project Project { get; set; } = null!;
    
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();
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
    
    public string? Details { get; set; }
    
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; } = null!;
}

public class Comment : BaseEntity
{
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public Guid IssueId { get; set; }
    public virtual Issue Issue { get; set; } = null!;
    
    public Guid AuthorId { get; set; }
    public virtual User Author { get; set; } = null!;
    
    public Guid? ParentCommentId { get; set; }
    public virtual Comment? ParentComment { get; set; }
    
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
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

public class IssueDependency : BaseEntity
{
    public Guid BlockingIssueId { get; set; }
    public virtual Issue BlockingIssue { get; set; } = null!;
    
    public Guid BlockedIssueId { get; set; }
    public virtual Issue BlockedIssue { get; set; } = null!;
    
    public DependencyType Type { get; set; } = DependencyType.Blocks;
}

public enum DependencyType
{
    Blocks = 1,
    Relates = 2,
    Duplicates = 3
}

public enum IssueStatus
{
    ToDo = 1,
    InProgress = 2,
    InReview = 3,
    Done = 4,
    Cancelled = 5
}
