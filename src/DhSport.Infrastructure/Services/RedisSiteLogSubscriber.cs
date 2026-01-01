using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using DhSport.Domain.Entities.Site;
using DhSport.Domain.Interfaces.Repositories;
using DhSport.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DhSport.Infrastructure.Services;

/// <summary>
/// Redis Pub/Sub을 구독하여 SiteLog를 배치로 저장하는 백그라운드 서비스
/// </summary>
public class RedisSiteLogSubscriber : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceProvider _serviceProvider;
    private readonly RedisOptions _options;
    private readonly ILogger<RedisSiteLogSubscriber> _logger;
    private readonly ConcurrentQueue<(SiteLog log, int retryCount)> _logQueue;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public RedisSiteLogSubscriber(
        IConnectionMultiplexer redis,
        IServiceProvider serviceProvider,
        IOptions<RedisOptions> options,
        ILogger<RedisSiteLogSubscriber> logger)
    {
        _redis = redis;
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
        _logQueue = new ConcurrentQueue<(SiteLog, int)>();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RedisSiteLogSubscriber starting...");

        try
        {
            var subscriber = _redis.GetSubscriber();
            
            // Redis 채널 구독
            await subscriber.SubscribeAsync(_options.SiteLogChannel, async (channel, message) =>
            {
                try
                {
                    var siteLog = JsonSerializer.Deserialize<SiteLog>(message!, _jsonOptions);
                    if (siteLog != null)
                    {
                        _logQueue.Enqueue((siteLog, 0));
                        _logger.LogDebug("Enqueued site log. Queue size: {QueueSize}", _logQueue.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize site log message");
                }
            });

            _logger.LogInformation("Subscribed to Redis channel: {Channel}", _options.SiteLogChannel);

            // 배치 처리 타이머
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.SiteLogBatchIntervalSeconds));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await timer.WaitForNextTickAsync(stoppingToken);
                    
                    // 배치 크기에 도달하거나 타이머 간격이 경과하면 처리
                    if (_logQueue.Count >= _options.SiteLogBatchSize || _logQueue.Count > 0)
                    {
                        await ProcessBatchAsync(stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // 정상 종료
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in batch processing loop");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in RedisSiteLogSubscriber");
        }
        finally
        {
            _logger.LogInformation("RedisSiteLogSubscriber stopped");
        }
    }

    /// <summary>
    /// 배치로 SiteLog 저장
    /// </summary>
    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        
        try
        {
            var batch = new List<(SiteLog log, int retryCount)>();
            var batchSize = Math.Min(_options.SiteLogBatchSize, _logQueue.Count);

            // 큐에서 배치 크기만큼 로그 추출
            for (int i = 0; i < batchSize; i++)
            {
                if (_logQueue.TryDequeue(out var item))
                {
                    batch.Add(item);
                }
            }

            if (batch.Count == 0)
            {
                return;
            }

            _logger.LogInformation("Processing batch of {Count} site logs. Queue remaining: {QueueSize}", 
                batch.Count, _logQueue.Count);

            var stopwatch = Stopwatch.StartNew();

            // Scoped 서비스 생성 (Repository는 Scoped)
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ISiteLogRepository>();

            var logs = batch.Select(x => x.log).ToList();
            var success = await repository.BulkInsertAsync(logs, cancellationToken);

            stopwatch.Stop();

            if (success)
            {
                _logger.LogInformation(
                    "Successfully bulk inserted {Count} site logs in {ElapsedMs}ms. Average: {AvgMs}ms/log",
                    batch.Count,
                    stopwatch.ElapsedMilliseconds,
                    stopwatch.ElapsedMilliseconds / (double)batch.Count);
            }
            else
            {
                // 실패 시 재시도
                var retryBatch = new List<(SiteLog log, int retryCount)>();
                var discardBatch = new List<SiteLog>();

                foreach (var (log, retryCount) in batch)
                {
                    if (retryCount < _options.SiteLogMaxRetries)
                    {
                        retryBatch.Add((log, retryCount + 1));
                    }
                    else
                    {
                        discardBatch.Add(log);
                    }
                }

                // 재시도 큐에 다시 추가
                foreach (var item in retryBatch)
                {
                    _logQueue.Enqueue(item);
                }

                if (retryBatch.Count > 0)
                {
                    _logger.LogWarning(
                        "Failed to insert batch. Re-queued {RetryCount} logs for retry. Discarded {DiscardCount} logs (max retries exceeded)",
                        retryBatch.Count,
                        discardBatch.Count);
                }

                if (discardBatch.Count > 0)
                {
                    _logger.LogError(
                        "Discarded {Count} site logs after {MaxRetries} retry attempts. First discarded log: SessionId={SessionId}, Url={Url}",
                        discardBatch.Count,
                        _options.SiteLogMaxRetries,
                        discardBatch.First().SessionId,
                        discardBatch.First().CurrUrl);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process batch");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RedisSiteLogSubscriber stopping. Processing remaining {Count} logs...", _logQueue.Count);

        // 남은 로그 처리
        while (_logQueue.Count > 0 && !cancellationToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }
}
