using System.IdentityModel.Tokens.Jwt;
using DhSport.API.DTOs;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DhSport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LikeLogsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public LikeLogsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 좋아요 로그 목록 조회
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<LikeLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLikeLogs(
        [FromQuery] LikeLogQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<LikeLog>().GetQueryable();

        // Apply filters
        if (parameters.PostId.HasValue)
            query = query.Where(ll => ll.PostId == parameters.PostId.Value);

        if (parameters.LikeUserId.HasValue)
            query = query.Where(ll => ll.LikeUserId == parameters.LikeUserId.Value);

        if (parameters.FromDate.HasValue)
            query = query.Where(ll => ll.CreateDttm >= parameters.FromDate.Value);

        if (parameters.ToDate.HasValue)
            query = query.Where(ll => ll.CreateDttm <= parameters.ToDate.Value);

        // Pagination
        var likeLogs = await query
            .OrderByDescending(ll => ll.CreateDttm)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = likeLogs.Select(ll => new LikeLogDto
        {
            Id = ll.Id,
            PostId = ll.PostId,
            LikeUserId = ll.LikeUserId,
            CreateDttm = ll.CreateDttm
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// 좋아요 로그 단건 조회
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LikeLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLikeLog(Guid id, CancellationToken cancellationToken)
    {
        var likeLog = await _unitOfWork.Repository<LikeLog>()
            .GetByIdAsync(id, cancellationToken);

        if (likeLog == null)
            return NotFound(new { message = "좋아요 로그를 찾을 수 없습니다." });

        var dto = new LikeLogDto
        {
            Id = likeLog.Id,
            PostId = likeLog.PostId,
            LikeUserId = likeLog.LikeUserId,
            CreateDttm = likeLog.CreateDttm
        };

        return Ok(dto);
    }

    /// <summary>
    /// 좋아요 로그 생성 (Post.LikeCnt 증가)
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(LikeLogDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateLikeLog(
        [FromBody] CreateLikeLogDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            // Extract user ID from JWT token
            var userId = GetUserIdFromToken();

            // Validate Post exists
            var post = await _unitOfWork.Repository<Post>()
                .GetByIdAsync(dto.PostId, cancellationToken);

            if (post == null)
                return NotFound(new { message = "존재하지 않는 게시글입니다." });

            if (post.IsDeleted)
                return BadRequest(new { message = "삭제된 게시글입니다." });

            // Create LikeLog
            var likeLog = new LikeLog
            {
                PostId = dto.PostId,
                LikeUserId = userId,
                CreateDttm = DateTime.UtcNow
            };

            await _unitOfWork.Repository<LikeLog>().AddAsync(likeLog, cancellationToken);

            // Increment Post.LikeCnt
            post.LikeCnt++;
            await _unitOfWork.Repository<Post>().UpdateAsync(post, cancellationToken);

            // Save all changes in single transaction
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var result = new LikeLogDto
            {
                Id = likeLog.Id,
                PostId = likeLog.PostId,
                LikeUserId = likeLog.LikeUserId,
                CreateDttm = likeLog.CreateDttm
            };

            return CreatedAtAction(nameof(GetLikeLog), new { id = likeLog.Id }, result);
        }
        catch (DbUpdateException ex)
        {
            // Handle unique constraint violation (duplicate like)
            if (ex.InnerException?.Message.Contains("duplicate key") == true ||
                ex.InnerException?.Message.Contains("unique constraint") == true ||
                ex.InnerException?.Message.Contains("IX_like_log_PostId_LikeUserId") == true)
            {
                return Conflict(new { message = "이미 좋아요를 누른 게시글입니다." });
            }
            throw;
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
