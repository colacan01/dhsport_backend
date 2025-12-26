namespace DhSport.Application.DTOs.Comment;

public class CreateCommentDto
{
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public object CommentContent { get; set; } = new();
}
