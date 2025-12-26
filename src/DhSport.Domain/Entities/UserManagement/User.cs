using DhSport.Domain.Entities.Common;

namespace DhSport.Domain.Entities.UserManagement;

public class User : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string LogonId { get; set; } = string.Empty;
    public string Passwd { get; set; } = string.Empty;
    public string UserNm { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Tel { get; set; }
    public string? ProfileImg { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime? LastLoginDttm { get; set; }

    // Audit fields
    public DateTime CreateDttm { get; set; }
    public Guid? CreateUserId { get; set; }
    public DateTime? UpdateDttm { get; set; }
    public Guid? UpdateUserId { get; set; }

    // Navigation properties
    public virtual ICollection<UserRoleMap> UserRoles { get; set; } = new List<UserRoleMap>();
}
