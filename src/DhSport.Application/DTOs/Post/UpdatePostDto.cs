namespace DhSport.Application.DTOs.Post;

public class UpdatePostDto
{
    public string? Title { get; set; }
    public object? PostContent { get; set; }
    public Guid? PostTypeId { get; set; }
    public bool? IsNotice { get; set; }
    public bool? IsSecret { get; set; }
}
