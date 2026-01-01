namespace DhSport.Application.DTOs.Post;

public class UpdatePostDto
{
    public string? Title { get; set; }
    public object? PostContent { get; set; }
    public Guid? PostTypeId { get; set; }
    public bool? IsNotice { get; set; }
    public bool? IsSecret { get; set; }
    
    /// <summary>
    /// 수정 사유 (리비전 저장용, 선택적)
    /// </summary>
    public string? RevisionNote { get; set; }
}
