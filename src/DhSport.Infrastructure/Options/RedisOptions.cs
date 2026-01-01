namespace DhSport.Infrastructure.Options;

/// <summary>
/// Redis 설정 옵션
/// </summary>
public class RedisOptions
{
    /// <summary>
    /// SiteLog Redis 채널명
    /// </summary>
    public string SiteLogChannel { get; set; } = "sitelog:queue";

    /// <summary>
    /// SiteLog 배치 크기 (한 번에 저장할 로그 개수)
    /// </summary>
    public int SiteLogBatchSize { get; set; } = 100;

    /// <summary>
    /// SiteLog 배치 간격 (초)
    /// </summary>
    public int SiteLogBatchIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// SiteLog 최대 재시도 횟수
    /// </summary>
    public int SiteLogMaxRetries { get; set; } = 3;
}
