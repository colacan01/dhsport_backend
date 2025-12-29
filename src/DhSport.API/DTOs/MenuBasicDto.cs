namespace DhSport.API.DTOs;

public class MenuBasicDto
{
    public Guid Id { get; set; }
    public string MenuNm { get; set; } = string.Empty;
    public string? MenuUrl { get; set; }
    public int DisplayOrder { get; set; }
}
