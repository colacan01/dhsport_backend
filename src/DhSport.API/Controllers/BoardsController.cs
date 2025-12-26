using DhSport.Application.DTOs.Board;
using DhSport.Application.Features.Boards.Commands.CreateBoard;
using DhSport.Application.Features.Boards.Commands.DeleteBoard;
using DhSport.Application.Features.Boards.Commands.UpdateBoard;
using DhSport.Application.Features.Boards.Queries.GetBoard;
using DhSport.Application.Features.Boards.Queries.GetBoards;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DhSport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BoardsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 게시판 목록 조회
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BoardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBoards(
        [FromQuery] Guid? boardTypeId = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var boards = await _mediator.Send(new GetBoardsQuery(boardTypeId, includeInactive), cancellationToken);
        return Ok(boards);
    }

    /// <summary>
    /// 게시판 상세 조회
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBoard(Guid id, CancellationToken cancellationToken)
    {
        var board = await _mediator.Send(new GetBoardQuery(id), cancellationToken);
        if (board == null)
            return NotFound(new { message = "Board not found" });

        return Ok(board);
    }

    /// <summary>
    /// 게시판 생성
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(BoardDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardDto createBoardDto, CancellationToken cancellationToken)
    {
        var board = await _mediator.Send(new CreateBoardCommand(createBoardDto), cancellationToken);
        return CreatedAtAction(nameof(GetBoard), new { id = board.Id }, board);
    }

    /// <summary>
    /// 게시판 수정
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(BoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateBoard(Guid id, [FromBody] UpdateBoardDto updateBoardDto, CancellationToken cancellationToken)
    {
        try
        {
            var board = await _mediator.Send(new UpdateBoardCommand(id, updateBoardDto), cancellationToken);
            return Ok(board);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 게시판 비활성화
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteBoard(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteBoardCommand(id), cancellationToken);
        if (!result)
            return NotFound(new { message = "Board not found" });

        return NoContent();
    }
}
