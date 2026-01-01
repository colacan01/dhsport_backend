namespace DhSport.Application.DTOs.SiteLog;

/// <summary>
/// SiteLog 응답 DTO
/// </summary>
public class SiteLogDto
{
    /// <summary>
    /// 로그 ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 세션 ID
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 사용자 ID (익명인 경우 null)
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
    /// 타임스탬프
    /// </summary>
    public DateTime CurrTimestamp { get; set; }
}
