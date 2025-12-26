using DhSport.Domain.Entities.Common;

namespace DhSport.Domain.Entities.Content;

public class BoardType : BaseEntity, IAuditableEntity
{
    public string BoardTypeNm { get; set; } = string.Empty;
    public string? BoardTypeDesc { get; set; }
    public bool IsActive { get; set; } = true;

    // Audit fields
    public DateTime CreateDttm { get; set; }
    public Guid? CreateUserId { get; set; }
    public DateTime? UpdateDttm { get; set; }
    public Guid? UpdateUserId { get; set; }

    // Navigation properties
    public virtual ICollection<Board> Boards { get; set; } = new List<Board>();
}
