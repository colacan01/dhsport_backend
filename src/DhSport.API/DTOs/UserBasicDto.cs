namespace DhSport.API.DTOs;

public class UserBasicDto
{
    public Guid Id { get; set; }
    public string LogonId { get; set; } = string.Empty;
    public string UserNm { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
