using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DhSport.Application.DTOs.Post;
using DhSport.Application.Features.Posts.Commands.CreatePost;
using DhSport.Application.Features.Posts.Commands.DeletePost;
using DhSport.Application.Features.Posts.Commands.LikePost;
using DhSport.Application.Features.Posts.Commands.UpdatePost;
using DhSport.Application.Features.Posts.Queries.GetPost;
using DhSport.Application.Features.Posts.Queries.GetPosts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DhSport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 게시물 목록 조회
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PostDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPosts(
        [FromQuery] Guid? boardId = null,
        [FromQuery] Guid? postTypeId = null,
        [FromQuery] string? searchKeyword = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var posts = await _mediator.Send(
            new GetPostsQuery(boardId, postTypeId, searchKeyword, pageNumber, pageSize),
            cancellationToken);

        return Ok(posts);
    }

    /// <summary>
    /// 게시물 상세 조회
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PostDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPost(Guid id, CancellationToken cancellationToken)
    {
        // Get user ID from token if authenticated
        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim))
                userId = Guid.Parse(userIdClaim);
        }

        // Get IP address
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var post = await _mediator.Send(new GetPostQuery(id, true, userId, ipAddress), cancellationToken);
        if (post == null)
            return NotFound(new { message = "Post not found" });

        return Ok(post);
    }

    /// <summary>
    /// 게시물 생성
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(PostDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostDto createPostDto, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromToken();
        var post = await _mediator.Send(new CreatePostCommand(createPostDto, userId), cancellationToken);
        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
    }

    /// <summary>
    /// 게시물 수정
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(PostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostDto updatePostDto, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var post = await _mediator.Send(new UpdatePostCommand(id, updatePostDto, userId), cancellationToken);
            return Ok(post);
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
    /// 게시물 삭제 (소프트 삭제)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePost(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var result = await _mediator.Send(new DeletePostCommand(id, userId), cancellationToken);
            if (!result)
                return NotFound(new { message = "Post not found" });

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    /// <summary>
    /// 게시물 좋아요/취소
    /// </summary>
    [HttpPost("{id}/like")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LikePost(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var isLiked = await _mediator.Send(new LikePostCommand(id, userId), cancellationToken);
            return Ok(new { isLiked, message = isLiked ? "Post liked" : "Post unliked" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
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
