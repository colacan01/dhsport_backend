using System.ComponentModel.DataAnnotations;

namespace DhSport.Application.DTOs.SiteLog;

/// <summary>
/// SiteLog 생성 DTO
/// </summary>
public class CreateSiteLogDto
{
    /// <summary>
    /// 현재 URL (필수)
    /// </summary>
    [Required(ErrorMessage = "현재 URL은 필수입니다.")]
    [MaxLength(1000)]
    public string CurrUrl { get; set; } = string.Empty;

    /// <summary>
    /// 이전 URL (선택)
    /// </summary>
    [MaxLength(1000)]
    public string? PrevUrl { get; set; }

    /// <summary>
    /// 이전 URL ID (선택)
    /// </summary>
    [MaxLength(200)]
    public string? PrevUrlId { get; set; }

    /// <summary>
    /// 현재 URL ID (선택)
    /// </summary>
    [MaxLength(200)]
    public string? CurrUrlId { get; set; }
}
