using System.IdentityModel.Tokens.Jwt;
using DhSport.API.DTOs;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Site;
using DhSport.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DhSport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    private static readonly string[] ValidNotificationTypes = new[]
    {
        nameof(NotificationType.Comment),
        nameof(NotificationType.Like),
        nameof(NotificationType.Mention),
        nameof(NotificationType.System)
    };

    public NotificationsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 현재 사용자의 알림 목록 조회
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool? isRead,
        [FromQuery] string? notificationType,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetUserIdFromToken();

        IQueryable<Notification> query = _unitOfWork.Repository<Notification>().GetQueryable();

        // 현재 사용자의 알림만
        query = query.Where(n => n.UserId == currentUserId);

        // 읽음 상태 필터
        if (isRead.HasValue)
            query = query.Where(n => n.IsRead == isRead.Value);

        // 타입 필터
        if (!string.IsNullOrWhiteSpace(notificationType))
            query = query.Where(n => n.NotificationType == notificationType);

        // 날짜 범위 필터
        if (startDate.HasValue)
            query = query.Where(n => n.CreateDttm >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(n => n.CreateDttm <= endDate.Value);

        // 검색 필터
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(n => n.NotificationTitle.Contains(search) ||
                                      n.NotificationContent.Contains(search));

        // 페이지네이션
        var notifications = await query
            .OrderByDescending(n => n.CreateDttm)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            NotificationType = n.NotificationType,
            NotificationTitle = n.NotificationTitle,
            NotificationContent = n.NotificationContent,
            IsRead = n.IsRead,
            CreateDttm = n.CreateDttm
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// 읽지 않은 알림 개수 조회
    /// </summary>
    [HttpGet("unread/count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var currentUserId = GetUserIdFromToken();

        var count = await _unitOfWork.Repository<Notification>()
            .GetQueryable()
            .Where(n => n.UserId == currentUserId && n.IsRead == false)
            .CountAsync(cancellationToken);

        return Ok(new { count });
    }

    /// <summary>
    /// 알림 단건 조회
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotification(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = GetUserIdFromToken();

        var notification = await _unitOfWork.Repository<Notification>()
            .GetByIdAsync(id, cancellationToken);

        if (notification == null)
            return NotFound(new { message = "알림을 찾을 수 없습니다." });

        // 사용자 격리 검증
        if (notification.UserId != currentUserId)
            return Forbid();

        var dto = new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            NotificationType = notification.NotificationType,
            NotificationTitle = notification.NotificationTitle,
            NotificationContent = notification.NotificationContent,
            IsRead = notification.IsRead,
            CreateDttm = notification.CreateDttm
        };

        return Ok(dto);
    }

    /// <summary>
    /// 알림 생성 (관리자 전용)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "관리자")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateNotification(
        [FromBody] CreateNotificationDto dto,
        CancellationToken cancellationToken)
    {
        // NotificationType 검증
        if (!IsValidNotificationType(dto.NotificationType))
            return BadRequest(new { message = "유효하지 않은 알림 타입입니다." });

        var notification = new Notification
        {
            UserId = dto.UserId,
            NotificationType = dto.NotificationType,
            NotificationTitle = dto.NotificationTitle,
            NotificationContent = dto.NotificationContent,
            IsRead = false,
            CreateDttm = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Notification>().AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            NotificationType = notification.NotificationType,
            NotificationTitle = notification.NotificationTitle,
            NotificationContent = notification.NotificationContent,
            IsRead = notification.IsRead,
            CreateDttm = notification.CreateDttm
        };

        return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, result);
    }

    /// <summary>
    /// 알림을 읽음으로 표시
    /// </summary>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = GetUserIdFromToken();

        var notification = await _unitOfWork.Repository<Notification>()
            .GetByIdAsync(id, cancellationToken);

        if (notification == null)
            return NotFound(new { message = "알림을 찾을 수 없습니다." });

        // 사용자 격리 검증
        if (notification.UserId != currentUserId)
            return Forbid();

        notification.IsRead = true;

        await _unitOfWork.Repository<Notification>().UpdateAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            NotificationType = notification.NotificationType,
            NotificationTitle = notification.NotificationTitle,
            NotificationContent = notification.NotificationContent,
            IsRead = notification.IsRead,
            CreateDttm = notification.CreateDttm
        };

        return Ok(dto);
    }

    /// <summary>
    /// 모든 알림을 읽음으로 표시
    /// </summary>
    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var currentUserId = GetUserIdFromToken();

        var unreadNotifications = await _unitOfWork.Repository<Notification>()
            .GetQueryable()
            .Where(n => n.UserId == currentUserId && n.IsRead == false)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            await _unitOfWork.Repository<Notification>().UpdateAsync(notification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new { count = unreadNotifications.Count, message = $"{unreadNotifications.Count}개의 알림을 읽음 처리했습니다." });
    }

    private Guid GetUserIdFromToken()
    {
        // Try standard "sub" claim first
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        // Fallback to ClaimTypes.NameIdentifier (mapped claim)
        if (string.IsNullOrEmpty(userIdClaim))
            userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(userIdClaim);
    }

    private bool IsValidNotificationType(string type)
    {
        return ValidNotificationTypes.Contains(type);
    }
}
