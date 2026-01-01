namespace DhSport.Application.DTOs.Post;

/// <summary>
/// 게시물 리비전 DTO
/// </summary>
public class PostRevisionDto
{
    /// <summary>
    /// 리비전 ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 게시물 ID
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// 게시물 내용 (JSON)
    /// </summary>
    public object? PostContent { get; set; }

    /// <summary>
    /// 수정 사유
    /// </summary>
    public string RevisionNote { get; set; } = string.Empty;

    /// <summary>
    /// 수정한 사용자 ID
    /// </summary>
    public Guid RevisionUserId { get; set; }

    /// <summary>
    /// 수정한 사용자 이름
    /// </summary>
    public string RevisionUserNm { get; set; } = string.Empty;

    /// <summary>
    /// 생성 일시
    /// </summary>
    public DateTime CreateDttm { get; set; }
}
