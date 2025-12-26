using DhSport.Application.Common.Interfaces;
using DhSport.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DhSport.Application.Common.Behaviours;

public class CachingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehaviour<TRequest, TResponse>> _logger;

    public CachingBehaviour(ICacheService cacheService, ILogger<CachingBehaviour<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICacheable cacheableRequest)
        {
            return await next();
        }

        var cacheKey = cacheableRequest.CacheKey;

        var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);

        if (cachedResponse != null)
        {
            _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogInformation("Cache miss for key: {CacheKey}", cacheKey);
        var response = await next();

        await _cacheService.SetAsync(cacheKey, response, cacheableRequest.Expiration, cancellationToken);

        return response;
    }
}
