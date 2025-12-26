namespace DhSport.Application.DTOs.User;

public class UpdateUserDto
{
    public string? UserNm { get; set; }
    public string? Email { get; set; }
    public string? Tel { get; set; }
    public string? ProfileImg { get; set; }
    public bool? IsActive { get; set; }
}
