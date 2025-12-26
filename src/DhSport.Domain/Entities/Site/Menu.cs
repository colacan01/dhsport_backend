using DhSport.Domain.Entities.Common;

namespace DhSport.Domain.Entities.Site;

public class Menu : BaseEntity, IAuditableEntity
{
    public Guid? ParentMenuId { get; set; }
    public string MenuNm { get; set; } = string.Empty;
    public string? MenuUrl { get; set; }
    public string? MenuIcon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Audit fields
    public DateTime CreateDttm { get; set; }
    public Guid? CreateUserId { get; set; }
    public DateTime? UpdateDttm { get; set; }
    public Guid? UpdateUserId { get; set; }

    // Navigation properties
    public virtual Menu? ParentMenu { get; set; }
    public virtual ICollection<Menu> ChildMenus { get; set; } = new List<Menu>();
}
