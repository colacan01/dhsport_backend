namespace DhSport.Application.DTOs.Auth;

public class RegisterDto
{
    public string LogonId { get; set; } = string.Empty;
    public string Passwd { get; set; } = string.Empty;
    public string UserNm { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Tel { get; set; }
    public string? ProfileImg { get; set; }
}
