using DhSport.Domain.Entities.Site;

namespace DhSport.Domain.Interfaces.Repositories;

/// <summary>
/// SiteLog Repository 인터페이스
/// </summary>
public interface ISiteLogRepository
{
    /// <summary>
    /// Bulk Insert로 여러 SiteLog를 한 번에 저장
    /// </summary>
    /// <param name="logs">저장할 SiteLog 컬렉션</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>성공 여부</returns>
    Task<bool> BulkInsertAsync(IEnumerable<SiteLog> logs, CancellationToken cancellationToken = default);
}
