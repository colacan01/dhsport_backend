namespace DhSport.Application.DTOs.Post;

public class PostTypeDto
{
    public Guid Id { get; set; }
    public string PostTypeNm { get; set; } = string.Empty;
    public string? PostTypeDesc { get; set; }
    public bool IsActive { get; set; }
}
