using System.IdentityModel.Tokens.Jwt;
using DhSport.API.DTOs;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using DhSport.Domain.Entities.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DhSport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadLogsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ReadLogsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 조회 로그 목록 조회 (PostId 필수)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReadLogs(
        [FromQuery] ReadLogQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        // Validate PostId is provided (required)
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var query = _unitOfWork.Repository<ReadLog>()
            .GetQueryable()
            .Where(rl => rl.PostId == parameters.PostId);

        // Apply optional filters
        if (parameters.ReadUserId.HasValue)
            query = query.Where(rl => rl.ReadUserId == parameters.ReadUserId.Value);

        if (parameters.FromDate.HasValue)
            query = query.Where(rl => rl.CreateDttm >= parameters.FromDate.Value);

        if (parameters.ToDate.HasValue)
            query = query.Where(rl => rl.CreateDttm <= parameters.ToDate.Value);

        // Pagination
        var readLogs = await query
            .OrderByDescending(rl => rl.CreateDttm)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = readLogs.Select(rl => new ReadLogDto
        {
            Id = rl.Id,
            PostId = rl.PostId,
            ReadUserId = rl.ReadUserId,
            IpAddress = rl.IpAddress,
            CreateDttm = rl.CreateDttm
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// 조회 로그 생성 (인증 선택사항)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReadLogDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateReadLog(
        [FromBody] CreateReadLogDto dto,
        CancellationToken cancellationToken)
    {
        // Validate Post exists
        var post = await _unitOfWork.Repository<Post>()
            .GetByIdAsync(dto.PostId, cancellationToken);

        if (post == null)
            return NotFound(new { message = "존재하지 않는 게시글입니다." });

        if (post.IsDeleted)
            return BadRequest(new { message = "삭제된 게시글입니다." });

        // Extract user ID if authenticated
        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim))
                userId = Guid.Parse(userIdClaim);
        }

        // Extract IP address from HTTP context
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        // Create ReadLog
        var readLog = new ReadLog
        {
            PostId = dto.PostId,
            ReadUserId = userId,
            IpAddress = ipAddress,
            CreateDttm = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ReadLog>().AddAsync(readLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new ReadLogDto
        {
            Id = readLog.Id,
            PostId = readLog.PostId,
            ReadUserId = readLog.ReadUserId,
            IpAddress = readLog.IpAddress,
            CreateDttm = readLog.CreateDttm
        };

        return CreatedAtAction(nameof(GetReadLogs), new { postId = readLog.PostId }, result);
    }
}
