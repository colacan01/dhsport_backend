namespace DhSport.Application.DTOs.User;

public class CreateUserDto
{
    public string LogonId { get; set; } = string.Empty;
    public string Passwd { get; set; } = string.Empty;
    public string UserNm { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Tel { get; set; }
    public string? ProfileImg { get; set; }
    public List<Guid>? RoleIds { get; set; }
}
