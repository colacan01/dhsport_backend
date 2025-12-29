using System.IdentityModel.Tokens.Jwt;
using DhSport.API.DTOs;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Site;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DhSport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenusController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public MenusController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 메뉴 목록 조회
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<MenuDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMenus(
        [FromQuery] Guid? parentMenuId,
        [FromQuery] bool rootOnly = false,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Menu> query = _unitOfWork.Repository<Menu>().GetQueryable();

        // 필터 적용
        if (parentMenuId.HasValue)
            query = query.Where(m => m.ParentMenuId == parentMenuId.Value);
        else if (rootOnly)
            query = query.Where(m => m.ParentMenuId == null);

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        // Include parent and children
        query = query.Include(m => m.ParentMenu)
                     .Include(m => m.ChildMenus);

        var menus = await query
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

        var dtos = menus.Select(m => new MenuDto
        {
            Id = m.Id,
            ParentMenuId = m.ParentMenuId,
            MenuNm = m.MenuNm,
            MenuUrl = m.MenuUrl,
            MenuIcon = m.MenuIcon,
            DisplayOrder = m.DisplayOrder,
            IsActive = m.IsActive,
            CreateDttm = m.CreateDttm,
            UpdateDttm = m.UpdateDttm,
            ParentMenu = m.ParentMenu != null ? new MenuBasicDto
            {
                Id = m.ParentMenu.Id,
                MenuNm = m.ParentMenu.MenuNm,
                MenuUrl = m.ParentMenu.MenuUrl,
                DisplayOrder = m.ParentMenu.DisplayOrder
            } : null,
            ChildMenus = m.ChildMenus?.Select(c => new MenuBasicDto
            {
                Id = c.Id,
                MenuNm = c.MenuNm,
                MenuUrl = c.MenuUrl,
                DisplayOrder = c.DisplayOrder
            }).ToList()
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// 메뉴 단건 조회
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(MenuDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMenu(Guid id, CancellationToken cancellationToken)
    {
        var menu = await _unitOfWork.Repository<Menu>()
            .GetQueryable()
            .Include(m => m.ParentMenu)
            .Include(m => m.ChildMenus)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (menu == null)
            return NotFound(new { message = "메뉴를 찾을 수 없습니다." });

        var dto = new MenuDto
        {
            Id = menu.Id,
            ParentMenuId = menu.ParentMenuId,
            MenuNm = menu.MenuNm,
            MenuUrl = menu.MenuUrl,
            MenuIcon = menu.MenuIcon,
            DisplayOrder = menu.DisplayOrder,
            IsActive = menu.IsActive,
            CreateDttm = menu.CreateDttm,
            UpdateDttm = menu.UpdateDttm,
            ParentMenu = menu.ParentMenu != null ? new MenuBasicDto
            {
                Id = menu.ParentMenu.Id,
                MenuNm = menu.ParentMenu.MenuNm,
                MenuUrl = menu.ParentMenu.MenuUrl,
                DisplayOrder = menu.ParentMenu.DisplayOrder
            } : null,
            ChildMenus = menu.ChildMenus?.Select(c => new MenuBasicDto
            {
                Id = c.Id,
                MenuNm = c.MenuNm,
                MenuUrl = c.MenuUrl,
                DisplayOrder = c.DisplayOrder
            }).ToList()
        };

        return Ok(dto);
    }

    /// <summary>
    /// 메뉴 생성 (관리자 전용)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "관리자")]
    [ProducesResponseType(typeof(MenuDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateMenu(
        [FromBody] CreateMenuDto dto,
        CancellationToken cancellationToken)
    {
        // ParentMenuId 유효성 검증
        if (dto.ParentMenuId.HasValue)
        {
            var parentExists = await _unitOfWork.Repository<Menu>()
                .GetByIdAsync(dto.ParentMenuId.Value, cancellationToken);

            if (parentExists == null)
                return BadRequest(new { message = "존재하지 않는 부모 메뉴입니다." });
        }

        var currentUserId = GetUserIdFromToken();

        var menu = new Menu
        {
            ParentMenuId = dto.ParentMenuId,
            MenuNm = dto.MenuNm,
            MenuUrl = dto.MenuUrl,
            MenuIcon = dto.MenuIcon,
            DisplayOrder = dto.DisplayOrder,
            IsActive = true,
            CreateDttm = DateTime.UtcNow,
            CreateUserId = currentUserId
        };

        await _unitOfWork.Repository<Menu>().AddAsync(menu, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with parent and children for response
        var created = await _unitOfWork.Repository<Menu>()
            .GetQueryable()
            .Include(m => m.ParentMenu)
            .Include(m => m.ChildMenus)
            .FirstOrDefaultAsync(m => m.Id == menu.Id, cancellationToken);

        var result = new MenuDto
        {
            Id = created!.Id,
            ParentMenuId = created.ParentMenuId,
            MenuNm = created.MenuNm,
            MenuUrl = created.MenuUrl,
            MenuIcon = created.MenuIcon,
            DisplayOrder = created.DisplayOrder,
            IsActive = created.IsActive,
            CreateDttm = created.CreateDttm,
            UpdateDttm = created.UpdateDttm,
            ParentMenu = created.ParentMenu != null ? new MenuBasicDto
            {
                Id = created.ParentMenu.Id,
                MenuNm = created.ParentMenu.MenuNm,
                MenuUrl = created.ParentMenu.MenuUrl,
                DisplayOrder = created.ParentMenu.DisplayOrder
            } : null,
            ChildMenus = created.ChildMenus?.Select(c => new MenuBasicDto
            {
                Id = c.Id,
                MenuNm = c.MenuNm,
                MenuUrl = c.MenuUrl,
                DisplayOrder = c.DisplayOrder
            }).ToList()
        };

        return CreatedAtAction(nameof(GetMenu), new { id = menu.Id }, result);
    }

    /// <summary>
    /// 메뉴 수정 (관리자 전용)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "관리자")]
    [ProducesResponseType(typeof(MenuDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMenu(
        Guid id,
        [FromBody] UpdateMenuDto dto,
        CancellationToken cancellationToken)
    {
        var menu = await _unitOfWork.Repository<Menu>()
            .GetQueryable()
            .Include(m => m.ParentMenu)
            .Include(m => m.ChildMenus)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (menu == null)
            return NotFound(new { message = "메뉴를 찾을 수 없습니다." });

        // 자기 자신을 부모로 설정 불가
        if (dto.ParentMenuId.HasValue && dto.ParentMenuId.Value == id)
            return BadRequest(new { message = "자기 자신을 부모 메뉴로 설정할 수 없습니다." });

        // 자손을 부모로 설정 불가 (순환 참조 방지)
        if (dto.ParentMenuId.HasValue)
        {
            var isDescendant = await IsDescendant(id, dto.ParentMenuId.Value, cancellationToken);
            if (isDescendant)
                return BadRequest(new { message = "자손 메뉴를 부모로 설정할 수 없습니다." });

            // ParentMenuId가 변경된 경우, 존재하는지 확인
            if (dto.ParentMenuId.Value != menu.ParentMenuId)
            {
                var parentExists = await _unitOfWork.Repository<Menu>()
                    .GetByIdAsync(dto.ParentMenuId.Value, cancellationToken);

                if (parentExists == null)
                    return BadRequest(new { message = "존재하지 않는 부모 메뉴입니다." });
            }
        }

        var currentUserId = GetUserIdFromToken();

        // Update properties
        menu.ParentMenuId = dto.ParentMenuId;
        menu.MenuNm = dto.MenuNm;
        menu.MenuUrl = dto.MenuUrl;
        menu.MenuIcon = dto.MenuIcon;
        menu.DisplayOrder = dto.DisplayOrder;
        menu.IsActive = dto.IsActive;
        menu.UpdateDttm = DateTime.UtcNow;
        menu.UpdateUserId = currentUserId;

        await _unitOfWork.Repository<Menu>().UpdateAsync(menu, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new MenuDto
        {
            Id = menu.Id,
            ParentMenuId = menu.ParentMenuId,
            MenuNm = menu.MenuNm,
            MenuUrl = menu.MenuUrl,
            MenuIcon = menu.MenuIcon,
            DisplayOrder = menu.DisplayOrder,
            IsActive = menu.IsActive,
            CreateDttm = menu.CreateDttm,
            UpdateDttm = menu.UpdateDttm,
            ParentMenu = menu.ParentMenu != null ? new MenuBasicDto
            {
                Id = menu.ParentMenu.Id,
                MenuNm = menu.ParentMenu.MenuNm,
                MenuUrl = menu.ParentMenu.MenuUrl,
                DisplayOrder = menu.ParentMenu.DisplayOrder
            } : null,
            ChildMenus = menu.ChildMenus?.Select(c => new MenuBasicDto
            {
                Id = c.Id,
                MenuNm = c.MenuNm,
                MenuUrl = c.MenuUrl,
                DisplayOrder = c.DisplayOrder
            }).ToList()
        };

        return Ok(result);
    }

    /// <summary>
    /// 메뉴 삭제 (관리자 전용, Soft Delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "관리자")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMenu(Guid id, CancellationToken cancellationToken)
    {
        var menu = await _unitOfWork.Repository<Menu>()
            .GetByIdAsync(id, cancellationToken);

        if (menu == null)
            return NotFound(new { message = "메뉴를 찾을 수 없습니다." });

        // 자식 메뉴가 있으면 삭제 거부
        var hasChildren = await _unitOfWork.Repository<Menu>()
            .GetQueryable()
            .AnyAsync(m => m.ParentMenuId == id && m.IsActive, cancellationToken);

        if (hasChildren)
            return BadRequest(new { message = "자식 메뉴가 있는 메뉴는 삭제할 수 없습니다." });

        var currentUserId = GetUserIdFromToken();

        // Soft delete
        menu.IsActive = false;
        menu.UpdateDttm = DateTime.UtcNow;
        menu.UpdateUserId = currentUserId;

        await _unitOfWork.Repository<Menu>().UpdateAsync(menu, cancellationToken);
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

    private async Task<bool> IsDescendant(Guid ancestorId, Guid potentialDescendantId, CancellationToken cancellationToken)
    {
        // 재귀적으로 자손인지 확인
        var menu = await _unitOfWork.Repository<Menu>()
            .GetQueryable()
            .Include(m => m.ChildMenus)
            .FirstOrDefaultAsync(m => m.Id == ancestorId, cancellationToken);

        if (menu == null || menu.ChildMenus == null || !menu.ChildMenus.Any())
            return false;

        // 직접 자식 중에 있는지 확인
        if (menu.ChildMenus.Any(c => c.Id == potentialDescendantId))
            return true;

        // 자손 중에 있는지 재귀 확인
        foreach (var child in menu.ChildMenus)
        {
            if (await IsDescendant(child.Id, potentialDescendantId, cancellationToken))
                return true;
        }

        return false;
    }
}
