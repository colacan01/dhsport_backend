namespace DhSport.Application.DTOs.SiteLog;

/// <summary>
/// SiteLog 조회 쿼리 파라미터
/// </summary>
public class SiteLogQueryParameters
{
    /// <summary>
    /// 세션 ID 필터
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// 사용자 ID 필터
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 시작 일시
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// 종료 일시
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// 페이지 번호
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 페이지 크기
    /// </summary>
    public int PageSize { get; set; } = 100;
}
