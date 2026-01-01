using DhSport.Domain.Entities.Common;

namespace DhSport.Domain.Entities.Site;

/// <summary>
/// 사이트 로그 엔티티 - 사용자의 페이지 탐색 기록 추적
/// </summary>
public class SiteLog : BaseEntity
{
    /// <summary>
    /// 세션 ID (웹서버 세션)
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 사용자 ID (익명 사용자 허용)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 이전 URL
    /// </summary>
    public string? PrevUrl { get; set; }

    /// <summary>
    /// 이전 URL ID
    /// </summary>
    public string? PrevUrlId { get; set; }

    /// <summary>
    /// 현재 URL
    /// </summary>
    public string CurrUrl { get; set; } = string.Empty;

    /// <summary>
    /// 현재 URL ID
    /// </summary>
    public string? CurrUrlId { get; set; }

    /// <summary>
    /// 현재 타임스탬프 (DB에서 자동 생성)
    /// </summary>
    public DateTime CurrTimestamp { get; set; }
}
