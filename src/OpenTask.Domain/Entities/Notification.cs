using System.ComponentModel.DataAnnotations;

namespace OpenTask.Domain.Entities;

public class Notification : BaseEntity
{
    [Required]
    public string Message { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;
    
    public bool IsRead { get; set; } = false;
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid? RelatedEntityId { get; set; }
    
    [MaxLength(100)]
    public string? RelatedEntityType { get; set; }
}
