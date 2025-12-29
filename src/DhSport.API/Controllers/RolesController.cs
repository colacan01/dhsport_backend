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
public class RolesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public RolesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 역할 목록 조회
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles([FromQuery] bool? isActive, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Role>().GetQueryable();

        // Filter by IsActive (default: true)
        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);
        else
            query = query.Where(r => r.IsActive);

        var roles = await query
            .OrderBy(r => r.RoleNm)
            .ToListAsync(cancellationToken);

        var dtos = roles.Select(r => new RoleDto
        {
            Id = r.Id,
            RoleNm = r.RoleNm,
            RoleDesc = r.RoleDesc,
            IsActive = r.IsActive,
            CreateDttm = r.CreateDttm,
            UpdateDttm = r.UpdateDttm
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// 역할 단건 조회
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRole(Guid id, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Repository<Role>()
            .GetByIdAsync(id, cancellationToken);

        if (role == null)
            return NotFound(new { message = "역할을 찾을 수 없습니다." });

        var dto = new RoleDto
        {
            Id = role.Id,
            RoleNm = role.RoleNm,
            RoleDesc = role.RoleDesc,
            IsActive = role.IsActive,
            CreateDttm = role.CreateDttm,
            UpdateDttm = role.UpdateDttm
        };

        return Ok(dto);
    }

    /// <summary>
    /// 역할 생성
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var role = new Role
            {
                RoleNm = dto.RoleNm,
                RoleDesc = dto.RoleDesc,
                IsActive = true,
                CreateDttm = DateTime.UtcNow,
                CreateUserId = GetUserIdFromToken()
            };

            await _unitOfWork.Repository<Role>().AddAsync(role, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var result = new RoleDto
            {
                Id = role.Id,
                RoleNm = role.RoleNm,
                RoleDesc = role.RoleDesc,
                IsActive = role.IsActive,
                CreateDttm = role.CreateDttm,
                UpdateDttm = role.UpdateDttm
            };

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, result);
        }
        catch (DbUpdateException ex)
        {
            // Handle unique constraint violation
            if (ex.InnerException?.Message.Contains("duplicate key") == true ||
                ex.InnerException?.Message.Contains("unique constraint") == true)
            {
                return BadRequest(new { message = "이미 존재하는 역할 이름입니다." });
            }
            throw;
        }
    }

    /// <summary>
    /// 역할 수정
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _unitOfWork.Repository<Role>()
                .GetByIdAsync(id, cancellationToken);

            if (role == null)
                return NotFound(new { message = "역할을 찾을 수 없습니다." });

            // Update properties
            role.RoleNm = dto.RoleNm;
            role.RoleDesc = dto.RoleDesc;
            role.IsActive = dto.IsActive;
            role.UpdateDttm = DateTime.UtcNow;
            role.UpdateUserId = GetUserIdFromToken();

            await _unitOfWork.Repository<Role>().UpdateAsync(role, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var result = new RoleDto
            {
                Id = role.Id,
                RoleNm = role.RoleNm,
                RoleDesc = role.RoleDesc,
                IsActive = role.IsActive,
                CreateDttm = role.CreateDttm,
                UpdateDttm = role.UpdateDttm
            };

            return Ok(result);
        }
        catch (DbUpdateException ex)
        {
            // Handle unique constraint violation
            if (ex.InnerException?.Message.Contains("duplicate key") == true ||
                ex.InnerException?.Message.Contains("unique constraint") == true)
            {
                return BadRequest(new { message = "이미 존재하는 역할 이름입니다." });
            }
            throw;
        }
    }

    /// <summary>
    /// 역할 삭제 (Soft Delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Repository<Role>()
            .GetByIdAsync(id, cancellationToken);

        if (role == null)
            return NotFound(new { message = "역할을 찾을 수 없습니다." });

        // Soft delete by setting IsActive to false
        role.IsActive = false;
        role.UpdateDttm = DateTime.UtcNow;
        role.UpdateUserId = GetUserIdFromToken();

        await _unitOfWork.Repository<Role>().UpdateAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(userIdClaim);
    }
}
