using DhSport.Domain.Entities.Common;

namespace DhSport.Domain.Entities.Content;

public class Board : BaseEntity, IAuditableEntity
{
    public Guid BoardTypeId { get; set; }
    public string BoardNm { get; set; } = string.Empty;
    public string? BoardDesc { get; set; }
    public string BoardSlug { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0; // Added display order
    public int PostsPerPage { get; set; } = 20;
    public bool IsActive { get; set; } = true;
    public bool AllowComment { get; set; } = true;
    public bool AllowAnonymous { get; set; } = false;
    public bool RequireApproval { get; set; } = false;

    // Audit fields
    public DateTime CreateDttm { get; set; }
    public Guid? CreateUserId { get; set; }
    public DateTime? UpdateDttm { get; set; }
    public Guid? UpdateUserId { get; set; }

    // Navigation properties
    public virtual BoardType BoardType { get; set; } = null!;
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
