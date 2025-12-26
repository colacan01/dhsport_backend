namespace DhSport.Application.DTOs.Post;

public class CreatePostDto
{
    public Guid BoardId { get; set; }
    public Guid PostTypeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public object? PostContent { get; set; }
    public bool IsNotice { get; set; } = false;
    public bool IsSecret { get; set; } = false;
}
