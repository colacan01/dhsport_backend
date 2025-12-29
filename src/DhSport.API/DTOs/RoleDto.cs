namespace DhSport.API.DTOs;

public class RoleDto
{
    public Guid Id { get; set; }
    public string RoleNm { get; set; } = string.Empty;
    public string? RoleDesc { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDttm { get; set; }
    public DateTime? UpdateDttm { get; set; }
}
