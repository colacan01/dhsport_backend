using System.Text.Json;
using DhSport.Application.DTOs.SiteLog;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Site;
using DhSport.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DhSport.Infrastructure.Services;

/// <summary>
/// Redis Pub/Sub을 통해 SiteLog를 발행하는 서비스
/// </summary>
public class RedisSiteLogPublisher : ISiteLogPublisher
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisOptions _options;
    private readonly ILogger<RedisSiteLogPublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisSiteLogPublisher(
        IConnectionMultiplexer redis,
        IOptions<RedisOptions> options,
        ILogger<RedisSiteLogPublisher> logger)
    {
        _redis = redis;
        _options = options.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// SiteLog를 Redis 채널로 발행
    /// </summary>
    public async Task PublishAsync(
        string sessionId, 
        Guid? userId, 
        CreateSiteLogDto dto, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var siteLog = new SiteLog
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                UserId = userId,
                PrevUrl = dto.PrevUrl,
                PrevUrlId = dto.PrevUrlId,
                CurrUrl = dto.CurrUrl,
                CurrUrlId = dto.CurrUrlId
                // CurrTimestamp는 DB에서 자동 생성
            };

            var message = JsonSerializer.Serialize(siteLog, _jsonOptions);
            var subscriber = _redis.GetSubscriber();
            
            await subscriber.PublishAsync(_options.SiteLogChannel, message);

            _logger.LogDebug("Published site log to Redis channel: {Channel}", _options.SiteLogChannel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish site log to Redis");
            throw;
        }
    }
}
