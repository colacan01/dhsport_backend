using DhSport.Application.DTOs.SiteLog;

namespace DhSport.Application.Interfaces;

/// <summary>
/// SiteLog Publisher 인터페이스 - Redis Pub/Sub을 통해 로그를 발행
/// </summary>
public interface ISiteLogPublisher
{
    /// <summary>
    /// SiteLog를 Redis 채널로 발행
    /// </summary>
    /// <param name="sessionId">세션 ID</param>
    /// <param name="userId">사용자 ID (익명인 경우 null)</param>
    /// <param name="dto">로그 DTO</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task PublishAsync(string sessionId, Guid? userId, CreateSiteLogDto dto, CancellationToken cancellationToken = default);
}
