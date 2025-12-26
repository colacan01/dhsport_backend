using DhSport.Domain.Entities.Common;

namespace DhSport.Domain.Entities.Content;

public class PostType : BaseEntity, IAuditableEntity
{
    public string PostTypeNm { get; set; } = string.Empty;
    public string? PostTypeDesc { get; set; }
    public bool IsActive { get; set; } = true;

    // Audit fields
    public DateTime CreateDttm { get; set; }
    public Guid? CreateUserId { get; set; }
    public DateTime? UpdateDttm { get; set; }
    public Guid? UpdateUserId { get; set; }

    // Navigation properties
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
