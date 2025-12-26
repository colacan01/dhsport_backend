using System.IdentityModel.Tokens.Jwt;
using DhSport.Application.DTOs.Comment;
using DhSport.Application.Features.Comments.Commands.CreateComment;
using DhSport.Application.Features.Comments.Commands.DeleteComment;
using DhSport.Application.Features.Comments.Commands.LikeComment;
using DhSport.Application.Features.Comments.Commands.UpdateComment;
using DhSport.Application.Features.Comments.Queries.GetComments;
using DhSport.Application.Features.Comments.Queries.GetComment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DhSport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 특정 게시물의 댓글 목록 조회
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DhSport.Application.DTOs.Post.CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetComments(
        [FromQuery] Guid? postId,
        CancellationToken cancellationToken)
    {
        if (!postId.HasValue)
            return BadRequest(new { message = "PostId is required" });

        var comments = await _mediator.Send(new GetCommentsQuery(postId.Value), cancellationToken);
        return Ok(comments);
    }

    /// <summary>
    /// 댓글 상세 조회
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DhSport.Application.DTOs.Post.CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComment(Guid id, CancellationToken cancellationToken)
    {
        var comment = await _mediator.Send(new GetCommentQuery(id), cancellationToken);

        if (comment == null)
            return NotFound(new { message = "Comment not found" });

        return Ok(comment);
    }

    /// <summary>
    /// 댓글 생성
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(DhSport.Application.DTOs.Post.CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto createCommentDto, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromToken();
        var comment = await _mediator.Send(new CreateCommentCommand(createCommentDto, userId), cancellationToken);
        return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
    }

    /// <summary>
    /// 댓글 수정
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(DhSport.Application.DTOs.Post.CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentDto updateCommentDto, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var comment = await _mediator.Send(new UpdateCommentCommand(id, updateCommentDto, userId), cancellationToken);
            return Ok(comment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    /// <summary>
    /// 댓글 삭제 (소프트 삭제)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteComment(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var result = await _mediator.Send(new DeleteCommentCommand(id, userId), cancellationToken);
            if (!result)
                return NotFound(new { message = "Comment not found" });

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    /// <summary>
    /// 댓글 좋아요/취소
    /// </summary>
    [HttpPost("{id}/like")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LikeComment(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var isLiked = await _mediator.Send(new LikeCommentCommand(id, userId), cancellationToken);
            return Ok(new { isLiked, message = isLiked ? "Comment liked" : "Comment unliked" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(userIdClaim);
    }
}
