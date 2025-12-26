using DhSport.Application.DTOs.Post;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DhSport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostTypesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public PostTypesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 게시물 타입 목록 조회
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PostTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPostTypes(CancellationToken cancellationToken)
    {
        var postTypes = await _unitOfWork.Repository<PostType>()
            .GetQueryable()
            .Where(pt => pt.IsActive)
            .OrderBy(pt => pt.PostTypeNm)
            .ToListAsync(cancellationToken);

        var dtos = postTypes.Select(pt => new PostTypeDto
        {
            Id = pt.Id,
            PostTypeNm = pt.PostTypeNm,
            PostTypeDesc = pt.PostTypeDesc,
            IsActive = pt.IsActive
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// 게시물 타입 단건 조회
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PostTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPostType(Guid id, CancellationToken cancellationToken)
    {
        var postType = await _unitOfWork.Repository<PostType>()
            .GetByIdAsync(id, cancellationToken);

        if (postType == null)
            return NotFound(new { message = "게시물 타입을 찾을 수 없습니다." });

        var dto = new PostTypeDto
        {
            Id = postType.Id,
            PostTypeNm = postType.PostTypeNm,
            PostTypeDesc = postType.PostTypeDesc,
            IsActive = postType.IsActive
        };

        return Ok(dto);
    }

    /// <summary>
    /// 게시물 타입 생성
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(PostTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePostType([FromBody] CreatePostTypeDto dto, CancellationToken cancellationToken)
    {
        var postType = new PostType
        {
            PostTypeNm = dto.PostTypeNm,
            PostTypeDesc = dto.PostTypeDesc,
            IsActive = true,
            CreateDttm = DateTime.UtcNow
        };

        await _unitOfWork.Repository<PostType>().AddAsync(postType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new PostTypeDto
        {
            Id = postType.Id,
            PostTypeNm = postType.PostTypeNm,
            PostTypeDesc = postType.PostTypeDesc,
            IsActive = postType.IsActive
        };

        return CreatedAtAction(nameof(GetPostType), new { id = postType.Id }, result);
    }

    /// <summary>
    /// 게시물 타입 수정
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(PostTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePostType(Guid id, [FromBody] UpdatePostTypeDto dto, CancellationToken cancellationToken)
    {
        var postType = await _unitOfWork.Repository<PostType>()
            .GetByIdAsync(id, cancellationToken);

        if (postType == null)
            return NotFound(new { message = "게시물 타입을 찾을 수 없습니다." });

        // Update properties
        if (!string.IsNullOrEmpty(dto.PostTypeNm))
            postType.PostTypeNm = dto.PostTypeNm;

        if (dto.PostTypeDesc != null)
            postType.PostTypeDesc = dto.PostTypeDesc;

        if (dto.IsActive.HasValue)
            postType.IsActive = dto.IsActive.Value;

        postType.UpdateDttm = DateTime.UtcNow;

        await _unitOfWork.Repository<PostType>().UpdateAsync(postType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new PostTypeDto
        {
            Id = postType.Id,
            PostTypeNm = postType.PostTypeNm,
            PostTypeDesc = postType.PostTypeDesc,
            IsActive = postType.IsActive
        };

        return Ok(result);
    }

    /// <summary>
    /// 게시물 타입 삭제 (Soft Delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeletePostType(Guid id, CancellationToken cancellationToken)
    {
        var postType = await _unitOfWork.Repository<PostType>()
            .GetByIdAsync(id, cancellationToken);

        if (postType == null)
            return NotFound(new { message = "게시물 타입을 찾을 수 없습니다." });

        // Soft delete by setting IsActive to false
        postType.IsActive = false;
        postType.UpdateDttm = DateTime.UtcNow;

        await _unitOfWork.Repository<PostType>().UpdateAsync(postType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public class CreatePostTypeDto
{
    public string PostTypeNm { get; set; } = string.Empty;
    public string? PostTypeDesc { get; set; }
}

public class UpdatePostTypeDto
{
    public string? PostTypeNm { get; set; }
    public string? PostTypeDesc { get; set; }
    public bool? IsActive { get; set; }
}
