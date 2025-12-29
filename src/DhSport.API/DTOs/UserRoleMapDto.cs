namespace DhSport.API.DTOs;

public class UserRoleMapDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDttm { get; set; }
    public DateTime? UpdateDttm { get; set; }

    // Nested information
    public UserBasicDto? User { get; set; }
    public RoleDto? Role { get; set; }
}
