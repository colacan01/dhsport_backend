namespace DhSport.Application.DTOs.User;

public class UserDto
{
    public Guid Id { get; set; }
    public string LogonId { get; set; } = string.Empty;
    public string UserNm { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Tel { get; set; }
    public string? ProfileImg { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginDttm { get; set; }
    public DateTime CreateDttm { get; set; }
    public List<string> Roles { get; set; } = new();
}
