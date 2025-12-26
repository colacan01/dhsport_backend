using DhSport.Application.DTOs.Board;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DhSport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoardTypesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public BoardTypesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 게시판 타입 목록 조회
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BoardTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBoardTypes(CancellationToken cancellationToken)
    {
        var boardTypes = await _unitOfWork.Repository<BoardType>()
            .GetQueryable()
            .Where(bt => bt.IsActive)
            .OrderBy(bt => bt.BoardTypeNm)
            .ToListAsync(cancellationToken);

        var dtos = boardTypes.Select(bt => new BoardTypeDto
        {
            Id = bt.Id,
            BoardTypeNm = bt.BoardTypeNm,
            BoardTypeDesc = bt.BoardTypeDesc,
            IsActive = bt.IsActive
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// 게시판 타입 단건 조회
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BoardTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBoardType(Guid id, CancellationToken cancellationToken)
    {
        var boardType = await _unitOfWork.Repository<BoardType>()
            .GetByIdAsync(id, cancellationToken);

        if (boardType == null)
            return NotFound(new { message = "게시판 타입을 찾을 수 없습니다." });

        var dto = new BoardTypeDto
        {
            Id = boardType.Id,
            BoardTypeNm = boardType.BoardTypeNm,
            BoardTypeDesc = boardType.BoardTypeDesc,
            IsActive = boardType.IsActive
        };

        return Ok(dto);
    }

    /// <summary>
    /// 게시판 타입 생성
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(BoardTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateBoardType([FromBody] CreateBoardTypeDto dto, CancellationToken cancellationToken)
    {
        var boardType = new BoardType
        {
            BoardTypeNm = dto.BoardTypeNm,
            BoardTypeDesc = dto.BoardTypeDesc,
            IsActive = true,
            CreateDttm = DateTime.UtcNow
        };

        await _unitOfWork.Repository<BoardType>().AddAsync(boardType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new BoardTypeDto
        {
            Id = boardType.Id,
            BoardTypeNm = boardType.BoardTypeNm,
            BoardTypeDesc = boardType.BoardTypeDesc,
            IsActive = boardType.IsActive
        };

        return CreatedAtAction(nameof(GetBoardType), new { id = boardType.Id }, result);
    }

    /// <summary>
    /// 게시판 타입 수정
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(BoardTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateBoardType(Guid id, [FromBody] UpdateBoardTypeDto dto, CancellationToken cancellationToken)
    {
        var boardType = await _unitOfWork.Repository<BoardType>()
            .GetByIdAsync(id, cancellationToken);

        if (boardType == null)
            return NotFound(new { message = "게시판 타입을 찾을 수 없습니다." });

        // Update properties
        if (!string.IsNullOrEmpty(dto.BoardTypeNm))
            boardType.BoardTypeNm = dto.BoardTypeNm;

        if (dto.BoardTypeDesc != null)
            boardType.BoardTypeDesc = dto.BoardTypeDesc;

        if (dto.IsActive.HasValue)
            boardType.IsActive = dto.IsActive.Value;

        boardType.UpdateDttm = DateTime.UtcNow;

        await _unitOfWork.Repository<BoardType>().UpdateAsync(boardType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new BoardTypeDto
        {
            Id = boardType.Id,
            BoardTypeNm = boardType.BoardTypeNm,
            BoardTypeDesc = boardType.BoardTypeDesc,
            IsActive = boardType.IsActive
        };

        return Ok(result);
    }

    /// <summary>
    /// 게시판 타입 삭제 (Soft Delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteBoardType(Guid id, CancellationToken cancellationToken)
    {
        var boardType = await _unitOfWork.Repository<BoardType>()
            .GetByIdAsync(id, cancellationToken);

        if (boardType == null)
            return NotFound(new { message = "게시판 타입을 찾을 수 없습니다." });

        // Soft delete by setting IsActive to false
        boardType.IsActive = false;
        boardType.UpdateDttm = DateTime.UtcNow;

        await _unitOfWork.Repository<BoardType>().UpdateAsync(boardType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public class CreateBoardTypeDto
{
    public string BoardTypeNm { get; set; } = string.Empty;
    public string? BoardTypeDesc { get; set; }
}

public class UpdateBoardTypeDto
{
    public string? BoardTypeNm { get; set; }
    public string? BoardTypeDesc { get; set; }
    public bool? IsActive { get; set; }
}
