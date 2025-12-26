namespace DhSport.Application.DTOs.Auth;

public class RegisterResponseDto
{
    public Guid UserId { get; set; }
    public string LogonId { get; set; } = string.Empty;
    public string UserNm { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreateDttm { get; set; }
}
