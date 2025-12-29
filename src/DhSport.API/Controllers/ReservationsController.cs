using System.IdentityModel.Tokens.Jwt;
using DhSport.API.DTOs;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Business;
using DhSport.Domain.Entities.UserManagement;
using DhSport.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DhSport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ReservationsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 예약 목록 조회
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetReservations(
        [FromQuery] ReservationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetUserIdFromToken();
        var isAdmin = await IsAdminUser(currentUserId, cancellationToken);

        IQueryable<Reservation> query = _unitOfWork.Repository<Reservation>()
            .GetQueryable();

        // 일반 사용자는 본인 예약만 조회
        if (!isAdmin)
        {
            query = query.Where(r => r.UserId == currentUserId);
        }
        else if (parameters.UserId.HasValue)
        {
            query = query.Where(r => r.UserId == parameters.UserId.Value);
        }

        // Apply filters
        if (parameters.ReservationType.HasValue)
            query = query.Where(r => r.ReservationType == parameters.ReservationType.Value.ToString());

        if (parameters.ReservationStatus.HasValue)
            query = query.Where(r => r.ReservationStatus == parameters.ReservationStatus.Value.ToString());

        if (parameters.StartDate.HasValue)
            query = query.Where(r => r.ReservationDttm >= parameters.StartDate.Value);

        if (parameters.EndDate.HasValue)
            query = query.Where(r => r.ReservationDttm <= parameters.EndDate.Value);

        // Pagination
        var reservations = await query
            .OrderByDescending(r => r.ReservationDttm)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = reservations.Select(r => new ReservationDto
        {
            Id = r.Id,
            UserId = r.UserId,
            ReservationType = Enum.Parse<ReservationType>(r.ReservationType),
            ReservationDttm = r.ReservationDttm,
            ReservationStatus = Enum.Parse<ReservationStatus>(r.ReservationStatus),
            CustomerNm = r.CustomerNm,
            CustomerTel = r.CustomerTel,
            CustomerEmail = r.CustomerEmail,
            ReservationNote = r.ReservationNote,
            AdminNote = r.AdminNote,
            CreateDttm = r.CreateDttm,
            UpdateDttm = r.UpdateDttm,
            User = null
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// 예약 단건 조회
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservation(Guid id, CancellationToken cancellationToken)
    {
        var reservation = await _unitOfWork.Repository<Reservation>()
            .GetQueryable()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (reservation == null)
            return NotFound(new { message = "예약을 찾을 수 없습니다." });

        // 소유권 검증
        var currentUserId = GetUserIdFromToken();
        var isAdmin = await IsAdminUser(currentUserId, cancellationToken);

        if (!isAdmin && reservation.UserId != currentUserId)
            return Forbid();

        var dto = new ReservationDto
        {
            Id = reservation.Id,
            UserId = reservation.UserId,
            ReservationType = Enum.Parse<ReservationType>(reservation.ReservationType),
            ReservationDttm = reservation.ReservationDttm,
            ReservationStatus = Enum.Parse<ReservationStatus>(reservation.ReservationStatus),
            CustomerNm = reservation.CustomerNm,
            CustomerTel = reservation.CustomerTel,
            CustomerEmail = reservation.CustomerEmail,
            ReservationNote = reservation.ReservationNote,
            AdminNote = reservation.AdminNote,
            CreateDttm = reservation.CreateDttm,
            UpdateDttm = reservation.UpdateDttm,
            User = null
        };

        return Ok(dto);
    }

    /// <summary>
    /// 예약 생성
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateReservation(
        [FromBody] CreateReservationDto dto,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetUserIdFromToken();

        var reservation = new Reservation
        {
            UserId = currentUserId,
            ReservationType = dto.ReservationType.ToString(),
            ReservationDttm = dto.ReservationDttm,
            ReservationStatus = ReservationStatus.Pending.ToString(),
            CustomerNm = dto.CustomerNm,
            CustomerTel = dto.CustomerTel,
            CustomerEmail = dto.CustomerEmail,
            ReservationNote = dto.ReservationNote,
            CreateDttm = DateTime.UtcNow,
            CreateUserId = currentUserId
        };

        await _unitOfWork.Repository<Reservation>().AddAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new ReservationDto
        {
            Id = reservation.Id,
            UserId = reservation.UserId,
            ReservationType = Enum.Parse<ReservationType>(reservation.ReservationType),
            ReservationDttm = reservation.ReservationDttm,
            ReservationStatus = Enum.Parse<ReservationStatus>(reservation.ReservationStatus),
            CustomerNm = reservation.CustomerNm,
            CustomerTel = reservation.CustomerTel,
            CustomerEmail = reservation.CustomerEmail,
            ReservationNote = reservation.ReservationNote,
            AdminNote = reservation.AdminNote,
            CreateDttm = reservation.CreateDttm,
            UpdateDttm = reservation.UpdateDttm,
            User = null
        };

        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, result);
    }

    /// <summary>
    /// 예약 수정
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReservation(
        Guid id,
        [FromBody] UpdateReservationDto dto,
        CancellationToken cancellationToken)
    {
        var reservation = await _unitOfWork.Repository<Reservation>()
            .GetQueryable()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (reservation == null)
            return NotFound(new { message = "예약을 찾을 수 없습니다." });

        // 소유권 검증
        var currentUserId = GetUserIdFromToken();
        var isAdmin = await IsAdminUser(currentUserId, cancellationToken);

        if (!isAdmin && reservation.UserId != currentUserId)
            return Forbid();

        // Update properties (Status는 PATCH 엔드포인트에서만 변경)
        reservation.ReservationType = dto.ReservationType.ToString();
        reservation.ReservationDttm = dto.ReservationDttm;
        reservation.CustomerNm = dto.CustomerNm;
        reservation.CustomerTel = dto.CustomerTel;
        reservation.CustomerEmail = dto.CustomerEmail;
        reservation.ReservationNote = dto.ReservationNote;
        reservation.UpdateDttm = DateTime.UtcNow;
        reservation.UpdateUserId = currentUserId;

        await _unitOfWork.Repository<Reservation>().UpdateAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new ReservationDto
        {
            Id = reservation.Id,
            UserId = reservation.UserId,
            ReservationType = Enum.Parse<ReservationType>(reservation.ReservationType),
            ReservationDttm = reservation.ReservationDttm,
            ReservationStatus = Enum.Parse<ReservationStatus>(reservation.ReservationStatus),
            CustomerNm = reservation.CustomerNm,
            CustomerTel = reservation.CustomerTel,
            CustomerEmail = reservation.CustomerEmail,
            ReservationNote = reservation.ReservationNote,
            AdminNote = reservation.AdminNote,
            CreateDttm = reservation.CreateDttm,
            UpdateDttm = reservation.UpdateDttm,
            User = null
        };

        return Ok(result);
    }

    /// <summary>
    /// 예약 상태 변경 (관리자 전용)
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "관리자")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReservationStatus(
        Guid id,
        [FromBody] UpdateReservationStatusDto dto,
        CancellationToken cancellationToken)
    {
        var reservation = await _unitOfWork.Repository<Reservation>()
            .GetQueryable()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (reservation == null)
            return NotFound(new { message = "예약을 찾을 수 없습니다." });

        var currentUserId = GetUserIdFromToken();

        // Update status and admin note
        reservation.ReservationStatus = dto.ReservationStatus.ToString();
        reservation.AdminNote = dto.AdminNote;
        reservation.UpdateDttm = DateTime.UtcNow;
        reservation.UpdateUserId = currentUserId;

        await _unitOfWork.Repository<Reservation>().UpdateAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new ReservationDto
        {
            Id = reservation.Id,
            UserId = reservation.UserId,
            ReservationType = Enum.Parse<ReservationType>(reservation.ReservationType),
            ReservationDttm = reservation.ReservationDttm,
            ReservationStatus = Enum.Parse<ReservationStatus>(reservation.ReservationStatus),
            CustomerNm = reservation.CustomerNm,
            CustomerTel = reservation.CustomerTel,
            CustomerEmail = reservation.CustomerEmail,
            ReservationNote = reservation.ReservationNote,
            AdminNote = reservation.AdminNote,
            CreateDttm = reservation.CreateDttm,
            UpdateDttm = reservation.UpdateDttm,
            User = null
        };

        return Ok(result);
    }

    /// <summary>
    /// 예약 삭제 (Soft Delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReservation(Guid id, CancellationToken cancellationToken)
    {
        var reservation = await _unitOfWork.Repository<Reservation>()
            .GetByIdAsync(id, cancellationToken);

        if (reservation == null)
            return NotFound(new { message = "예약을 찾을 수 없습니다." });

        // 소유권 검증
        var currentUserId = GetUserIdFromToken();
        var isAdmin = await IsAdminUser(currentUserId, cancellationToken);

        if (!isAdmin && reservation.UserId != currentUserId)
            return Forbid();

        // Soft delete
        reservation.IsDeleted = true;
        reservation.UpdateDttm = DateTime.UtcNow;
        reservation.UpdateUserId = currentUserId;

        await _unitOfWork.Repository<Reservation>().UpdateAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
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

    private async Task<bool> IsAdminUser(Guid userId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<UserRoleMap>()
            .GetQueryable()
            .Include(urm => urm.Role)
            .AnyAsync(urm =>
                urm.UserId == userId &&
                urm.IsActive &&
                urm.Role.RoleNm == "관리자" &&
                urm.Role.IsActive,
                cancellationToken);
    }
}
