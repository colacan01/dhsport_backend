namespace DhSport.Application.DTOs.Post;

/// <summary>
/// 게시물 리비전 차이 비교 DTO
/// </summary>
public class PostRevisionDiffDto
{
    /// <summary>
    /// 비교 대상 시작 리비전 ID
    /// </summary>
    public Guid FromRevisionId { get; set; }

    /// <summary>
    /// 비교 대상 종료 리비전 ID
    /// </summary>
    public Guid ToRevisionId { get; set; }

    /// <summary>
    /// 추가된 필드 (키: 필드명, 값: ToRevision의 값)
    /// </summary>
    public Dictionary<string, object?> AddedFields { get; set; } = new();

    /// <summary>
    /// 수정된 필드 (키: 필드명, 값: { from: 이전값, to: 새값 })
    /// </summary>
    public Dictionary<string, ModifiedFieldValue> ModifiedFields { get; set; } = new();

    /// <summary>
    /// 삭제된 필드 (키: 필드명, 값: FromRevision의 값)
    /// </summary>
    public Dictionary<string, object?> RemovedFields { get; set; } = new();
}

/// <summary>
/// 수정된 필드의 이전/이후 값
/// </summary>
public class ModifiedFieldValue
{
    public object? From { get; set; }
    public object? To { get; set; }
}
