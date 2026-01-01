using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DhSport.Application.DTOs.SiteLog;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Site;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DhSport.API.Controllers;

/// <summary>
/// 사이트 로그 관리 컨트롤러
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SiteLogsController : ControllerBase
{
    private readonly ISiteLogPublisher _publisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SiteLogsController> _logger;

    public SiteLogsController(
        ISiteLogPublisher publisher,
        IUnitOfWork unitOfWork,
        ILogger<SiteLogsController> logger)
    {
        _publisher = publisher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// 사이트 로그 생성 (익명 사용자 허용)
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSiteLog(
        [FromBody] CreateSiteLogDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 세션 초기화 및 세션 ID 추출
            if (!HttpContext.Session.IsAvailable || string.IsNullOrEmpty(HttpContext.Session.Id))
            {
                HttpContext.Session.SetString("_init", "1");
                await HttpContext.Session.CommitAsync(cancellationToken);
            }

            var sessionId = HttpContext.Session.Id;

            // JWT 토큰에서 사용자 ID 추출 (인증된 경우에만)
            Guid? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }

                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            // Redis Pub/Sub으로 로그 발행
            await _publisher.PublishAsync(sessionId, userId, dto, cancellationToken);

            _logger.LogDebug(
                "Site log published. SessionId: {SessionId}, UserId: {UserId}, Url: {Url}",
                sessionId,
                userId?.ToString() ?? "anonymous",
                dto.CurrUrl);

            return Accepted(new { message = "로그가 비동기로 처리됩니다." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish site log");
            return StatusCode(500, new { message = "로그 발행에 실패했습니다." });
        }
    }

    /// <summary>
    /// 사이트 로그 목록 조회 (관리자 전용)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "관리자")]
    [ProducesResponseType(typeof(List<SiteLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSiteLogs(
        [FromQuery] SiteLogQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<SiteLog> query = _unitOfWork.Repository<SiteLog>().GetQueryable();

            // 필터링
            if (!string.IsNullOrWhiteSpace(parameters.SessionId))
            {
                query = query.Where(s => s.SessionId == parameters.SessionId);
            }

            if (parameters.UserId.HasValue)
            {
                query = query.Where(s => s.UserId == parameters.UserId.Value);
            }

            if (parameters.FromDate.HasValue)
            {
                query = query.Where(s => s.CurrTimestamp >= parameters.FromDate.Value);
            }

            if (parameters.ToDate.HasValue)
            {
                query = query.Where(s => s.CurrTimestamp <= parameters.ToDate.Value);
            }

            // 정렬 및 페이지네이션
            var siteLogs = await query
                .OrderByDescending(s => s.CurrTimestamp)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync(cancellationToken);

            // DTO 변환
            var dtos = siteLogs.Select(s => new SiteLogDto
            {
                Id = s.Id,
                SessionId = s.SessionId,
                UserId = s.UserId,
                PrevUrl = s.PrevUrl,
                PrevUrlId = s.PrevUrlId,
                CurrUrl = s.CurrUrl,
                CurrUrlId = s.CurrUrlId,
                CurrTimestamp = s.CurrTimestamp
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve site logs");
            return StatusCode(500, new { message = "로그 조회에 실패했습니다." });
        }
    }

    /// <summary>
    /// 사이트 로그 단건 조회 (관리자 전용)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "관리자")]
    [ProducesResponseType(typeof(SiteLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSiteLog(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var siteLog = await _unitOfWork.Repository<SiteLog>()
                .GetByIdAsync(id, cancellationToken);

            if (siteLog == null)
            {
                return NotFound(new { message = "로그를 찾을 수 없습니다." });
            }

            var dto = new SiteLogDto
            {
                Id = siteLog.Id,
                SessionId = siteLog.SessionId,
                UserId = siteLog.UserId,
                PrevUrl = siteLog.PrevUrl,
                PrevUrlId = siteLog.PrevUrlId,
                CurrUrl = siteLog.CurrUrl,
                CurrUrlId = siteLog.CurrUrlId,
                CurrTimestamp = siteLog.CurrTimestamp
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve site log {LogId}", id);
            return StatusCode(500, new { message = "로그 조회에 실패했습니다." });
        }
    }
}
