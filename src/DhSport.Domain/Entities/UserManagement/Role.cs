using DhSport.Domain.Entities.Common;

namespace DhSport.Domain.Entities.UserManagement;

public class Role : BaseEntity, IAuditableEntity
{
    public string RoleNm { get; set; } = string.Empty;
    public string? RoleDesc { get; set; }
    public bool IsActive { get; set; } = true;

    // Audit fields
    public DateTime CreateDttm { get; set; }
    public Guid? CreateUserId { get; set; }
    public DateTime? UpdateDttm { get; set; }
    public Guid? UpdateUserId { get; set; }

    // Navigation properties
    public virtual ICollection<UserRoleMap> UserRoles { get; set; } = new List<UserRoleMap>();
}
