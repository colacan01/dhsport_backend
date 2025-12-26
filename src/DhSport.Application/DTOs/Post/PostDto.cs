namespace DhSport.Application.DTOs.Post;

public class PostDto
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string BoardNm { get; set; } = string.Empty;
    public Guid PostTypeId { get; set; }
    public string PostTypeNm { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public object? PostContent { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorNm { get; set; } = string.Empty;
    public bool IsNotice { get; set; }
    public bool IsSecret { get; set; }
    public int ViewCnt { get; set; }
    public int LikeCnt { get; set; }
    public int CommentCnt { get; set; }
    public DateTime CreateDttm { get; set; }
    public DateTime? UpdateDttm { get; set; }
}
