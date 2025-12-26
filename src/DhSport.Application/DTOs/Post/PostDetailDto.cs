namespace DhSport.Application.DTOs.Post;

public class PostDetailDto : PostDto
{
    public List<PostFileDto> Files { get; set; } = new();
    public List<CommentDto> Comments { get; set; } = new();
}

public class PostFileDto
{
    public Guid Id { get; set; }
    public string FileNm { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? FileType { get; set; }
    public int DisplayOrder { get; set; }
}

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid? ParentCommentId { get; set; }
    public object CommentContent { get; set; } = new();
    public Guid AuthorId { get; set; }
    public string AuthorNm { get; set; } = string.Empty;
    public int LikeCnt { get; set; }
    public DateTime CreateDttm { get; set; }
    public DateTime? UpdateDttm { get; set; }
    public List<CommentDto> Replies { get; set; } = new();
}
