using System.IdentityModel.Tokens.Jwt;
using DhSport.API.DTOs;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DhSport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserRoleMapsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public UserRoleMapsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 사용자-역할 매핑 목록 조회
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserRoleMapDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRoleMaps(
        [FromQuery] UserRoleMapQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        IQueryable<UserRoleMap> query = _unitOfWork.Repository<UserRoleMap>()
            .GetQueryable();

        // Apply filters
        if (parameters.UserId.HasValue)
            query = query.Where(urm => urm.UserId == parameters.UserId.Value);

        if (parameters.RoleId.HasValue)
            query = query.Where(urm => urm.RoleId == parameters.RoleId.Value);

        if (parameters.IsActive.HasValue)
            query = query.Where(urm => urm.IsActive == parameters.IsActive.Value);

        // Apply includes after filters
        query = query.Include(urm => urm.User).Include(urm => urm.Role);

        // Pagination
        var userRoleMaps = await query
            .OrderByDescending(urm => urm.CreateDttm)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = userRoleMaps.Select(urm => new UserRoleMapDto
        {
            Id = urm.Id,
            UserId = urm.UserId,
            RoleId = urm.RoleId,
            IsActive = urm.IsActive,
            CreateDttm = urm.CreateDttm,
            UpdateDttm = urm.UpdateDttm,
            User = urm.User != null ? new UserBasicDto
            {
                Id = urm.User.Id,
                LogonId = urm.User.LogonId,
                UserNm = urm.User.UserNm,
                Email = urm.User.Email
            } : null,
            Role = urm.Role != null ? new RoleDto
            {
                Id = urm.Role.Id,
                RoleNm = urm.Role.RoleNm,
                RoleDesc = urm.Role.RoleDesc,
                IsActive = urm.Role.IsActive,
                CreateDttm = urm.Role.CreateDttm,
                UpdateDttm = urm.Role.UpdateDttm
            } : null
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// 사용자-역할 매핑 단건 조회
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserRoleMapDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserRoleMap(Guid id, CancellationToken cancellationToken)
    {
        var userRoleMap = await _unitOfWork.Repository<UserRoleMap>()
            .GetQueryable()
            .Include(urm => urm.User)
            .Include(urm => urm.Role)
            .FirstOrDefaultAsync(urm => urm.Id == id, cancellationToken);

        if (userRoleMap == null)
            return NotFound(new { message = "사용자-역할 매핑을 찾을 수 없습니다." });

        var dto = new UserRoleMapDto
        {
            Id = userRoleMap.Id,
            UserId = userRoleMap.UserId,
            RoleId = userRoleMap.RoleId,
            IsActive = userRoleMap.IsActive,
            CreateDttm = userRoleMap.CreateDttm,
            UpdateDttm = userRoleMap.UpdateDttm,
            User = userRoleMap.User != null ? new UserBasicDto
            {
                Id = userRoleMap.User.Id,
                LogonId = userRoleMap.User.LogonId,
                UserNm = userRoleMap.User.UserNm,
                Email = userRoleMap.User.Email
            } : null,
            Role = userRoleMap.Role != null ? new RoleDto
            {
                Id = userRoleMap.Role.Id,
                RoleNm = userRoleMap.Role.RoleNm,
                RoleDesc = userRoleMap.Role.RoleDesc,
                IsActive = userRoleMap.Role.IsActive,
                CreateDttm = userRoleMap.Role.CreateDttm,
                UpdateDttm = userRoleMap.Role.UpdateDttm
            } : null
        };

        return Ok(dto);
    }

    /// <summary>
    /// 사용자에게 역할 할당
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(UserRoleMapDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUserRoleMap(
        [FromBody] CreateUserRoleMapDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate User exists
            var user = await _unitOfWork.Repository<User>()
                .GetByIdAsync(dto.UserId, cancellationToken);

            if (user == null)
                return NotFound(new { message = "존재하지 않는 사용자입니다." });

            if (user.IsDeleted)
                return BadRequest(new { message = "삭제된 사용자입니다." });

            if (!user.IsActive)
                return BadRequest(new { message = "비활성 사용자입니다." });

            // Validate Role exists
            var role = await _unitOfWork.Repository<Role>()
                .GetByIdAsync(dto.RoleId, cancellationToken);

            if (role == null)
                return NotFound(new { message = "존재하지 않는 역할입니다." });

            if (!role.IsActive)
                return BadRequest(new { message = "비활성 역할입니다." });

            // Create UserRoleMap
            var userRoleMap = new UserRoleMap
            {
                UserId = dto.UserId,
                RoleId = dto.RoleId,
                IsActive = true,
                CreateDttm = DateTime.UtcNow,
                CreateUserId = GetUserIdFromToken()
            };

            await _unitOfWork.Repository<UserRoleMap>().AddAsync(userRoleMap, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with User and Role for response
            var created = await _unitOfWork.Repository<UserRoleMap>()
                .GetQueryable()
                .Include(urm => urm.User)
                .Include(urm => urm.Role)
                .FirstOrDefaultAsync(urm => urm.Id == userRoleMap.Id, cancellationToken);

            var result = new UserRoleMapDto
            {
                Id = created!.Id,
                UserId = created.UserId,
                RoleId = created.RoleId,
                IsActive = created.IsActive,
                CreateDttm = created.CreateDttm,
                UpdateDttm = created.UpdateDttm,
                User = created.User != null ? new UserBasicDto
                {
                    Id = created.User.Id,
                    LogonId = created.User.LogonId,
                    UserNm = created.User.UserNm,
                    Email = created.User.Email
                } : null,
                Role = created.Role != null ? new RoleDto
                {
                    Id = created.Role.Id,
                    RoleNm = created.Role.RoleNm,
                    RoleDesc = created.Role.RoleDesc,
                    IsActive = created.Role.IsActive,
                    CreateDttm = created.Role.CreateDttm,
                    UpdateDttm = created.Role.UpdateDttm
                } : null
            };

            return CreatedAtAction(nameof(GetUserRoleMap), new { id = userRoleMap.Id }, result);
        }
        catch (DbUpdateException ex)
        {
            // Handle unique constraint violation (duplicate role assignment)
            if (ex.InnerException?.Message.Contains("duplicate key") == true ||
                ex.InnerException?.Message.Contains("unique constraint") == true)
            {
                return Conflict(new { message = "이미 해당 역할이 할당되어 있습니다." });
            }
            throw;
        }
    }

    /// <summary>
    /// 사용자-역할 매핑 수정
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(UserRoleMapDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateUserRoleMap(
        Guid id,
        [FromBody] UpdateUserRoleMapDto dto,
        CancellationToken cancellationToken)
    {
        var userRoleMap = await _unitOfWork.Repository<UserRoleMap>()
            .GetQueryable()
            .Include(urm => urm.User)
            .Include(urm => urm.Role)
            .FirstOrDefaultAsync(urm => urm.Id == id, cancellationToken);

        if (userRoleMap == null)
            return NotFound(new { message = "사용자-역할 매핑을 찾을 수 없습니다." });

        // Update properties
        userRoleMap.IsActive = dto.IsActive;
        userRoleMap.UpdateDttm = DateTime.UtcNow;
        userRoleMap.UpdateUserId = GetUserIdFromToken();

        await _unitOfWork.Repository<UserRoleMap>().UpdateAsync(userRoleMap, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new UserRoleMapDto
        {
            Id = userRoleMap.Id,
            UserId = userRoleMap.UserId,
            RoleId = userRoleMap.RoleId,
            IsActive = userRoleMap.IsActive,
            CreateDttm = userRoleMap.CreateDttm,
            UpdateDttm = userRoleMap.UpdateDttm,
            User = userRoleMap.User != null ? new UserBasicDto
            {
                Id = userRoleMap.User.Id,
                LogonId = userRoleMap.User.LogonId,
                UserNm = userRoleMap.User.UserNm,
                Email = userRoleMap.User.Email
            } : null,
            Role = userRoleMap.Role != null ? new RoleDto
            {
                Id = userRoleMap.Role.Id,
                RoleNm = userRoleMap.Role.RoleNm,
                RoleDesc = userRoleMap.Role.RoleDesc,
                IsActive = userRoleMap.Role.IsActive,
                CreateDttm = userRoleMap.Role.CreateDttm,
                UpdateDttm = userRoleMap.Role.UpdateDttm
            } : null
        };

        return Ok(result);
    }

    /// <summary>
    /// 사용자-역할 매핑 삭제 (Hard Delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteUserRoleMap(Guid id, CancellationToken cancellationToken)
    {
        var userRoleMap = await _unitOfWork.Repository<UserRoleMap>()
            .GetByIdAsync(id, cancellationToken);

        if (userRoleMap == null)
            return NotFound(new { message = "사용자-역할 매핑을 찾을 수 없습니다." });

        // Hard delete (UserRoleMap doesn't implement ISoftDeletable)
        await _unitOfWork.Repository<UserRoleMap>().DeleteAsync(userRoleMap, cancellationToken);
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
}
