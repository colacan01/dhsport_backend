using DhSport.Domain.Entities.Site;
using DhSport.Domain.Interfaces.Repositories;
using DhSport.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DhSport.Infrastructure.Repositories;

/// <summary>
/// SiteLog Repository 구현
/// </summary>
public class SiteLogRepository : Repository<SiteLog>, ISiteLogRepository
{
    private readonly ILogger<SiteLogRepository> _logger;

    public SiteLogRepository(ApplicationDbContext context, ILogger<SiteLogRepository> logger) 
        : base(context)
    {
        _logger = logger;
    }

    /// <summary>
    /// Bulk Insert로 여러 SiteLog를 한 번에 저장
    /// </summary>
    public async Task<bool> BulkInsertAsync(IEnumerable<SiteLog> logs, CancellationToken cancellationToken = default)
    {
        try
        {
            var logList = logs.ToList();
            if (logList.Count == 0)
            {
                return true;
            }

            // AddRangeAsync로 배치 삽입
            await _context.SiteLogs.AddRangeAsync(logList, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // DB에서 생성된 CurrTimestamp 값을 엔티티에 반영
            foreach (var log in logList)
            {
                await _context.Entry(log).ReloadAsync(cancellationToken);
            }

            _logger.LogInformation("Successfully bulk inserted {Count} site logs", logList.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bulk insert site logs");
            return false;
        }
    }
}
