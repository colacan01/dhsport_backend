namespace DhSport.API.DTOs;

public class UserRoleMapQueryParameters
{
    public Guid? UserId { get; set; }
    public Guid? RoleId { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
