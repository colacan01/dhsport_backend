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
public class AddFeaturesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AddFeaturesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 추가 기능 목록 조회
    /// </summary>
    /// <param name="postId">게시글 ID로 필터링 (선택사항)</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<AddFeatureDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAddFeatures([FromQuery] Guid? postId, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<AddFeature>()
            .GetQueryable()
            .Include(af => af.FeatureType)
            .Where(af => !af.IsDeleted);

        if (postId.HasValue)
        {
            query = query.Where(af => af.PostId == postId.Value);
        }

        var addFeatures = await query
            .OrderByDescending(af => af.CreateDttm)
            .ToListAsync(cancellationToken);

        var dtos = addFeatures.Select(af => new AddFeatureDto
        {
            Id = af.Id,
            PostId = af.PostId,
            FeatureTypeId = af.FeatureTypeId,
            FeatureContent = af.FeatureContent,
            IsDeleted = af.IsDeleted,
            CreateDttm = af.CreateDttm,
            UpdateDttm = af.UpdateDttm,
            FeatureType = af.FeatureType != null ? new FeatureTypeDto
            {
                Id = af.FeatureType.Id,
                FeatureTypeNm = af.FeatureType.FeatureTypeNm,
                FeatureTypeDesc = af.FeatureType.FeatureTypeDesc,
                IsActive = af.FeatureType.IsActive,
                DisplayOrder = af.FeatureType.DisplayOrder,
                CreateDttm = af.FeatureType.CreateDttm,
                UpdateDttm = af.FeatureType.UpdateDttm
            } : null
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// 추가 기능 단건 조회
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AddFeatureDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAddFeature(Guid id, CancellationToken cancellationToken)
    {
        var addFeature = await _unitOfWork.Repository<AddFeature>()
            .GetQueryable()
            .Include(af => af.FeatureType)
            .FirstOrDefaultAsync(af => af.Id == id, cancellationToken);

        if (addFeature == null)
            return NotFound(new { message = "추가 기능을 찾을 수 없습니다." });

        var dto = new AddFeatureDto
        {
            Id = addFeature.Id,
            PostId = addFeature.PostId,
            FeatureTypeId = addFeature.FeatureTypeId,
            FeatureContent = addFeature.FeatureContent,
            IsDeleted = addFeature.IsDeleted,
            CreateDttm = addFeature.CreateDttm,
            UpdateDttm = addFeature.UpdateDttm,
            FeatureType = addFeature.FeatureType != null ? new FeatureTypeDto
            {
                Id = addFeature.FeatureType.Id,
                FeatureTypeNm = addFeature.FeatureType.FeatureTypeNm,
                FeatureTypeDesc = addFeature.FeatureType.FeatureTypeDesc,
                IsActive = addFeature.FeatureType.IsActive,
                DisplayOrder = addFeature.FeatureType.DisplayOrder,
                CreateDttm = addFeature.FeatureType.CreateDttm,
                UpdateDttm = addFeature.FeatureType.UpdateDttm
            } : null
        };

        return Ok(dto);
    }

    /// <summary>
    /// 추가 기능 생성
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(AddFeatureDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateAddFeature([FromBody] CreateAddFeatureDto dto, CancellationToken cancellationToken)
    {
        // Validate PostId exists
        var postExists = await _unitOfWork.Repository<Post>()
            .GetByIdAsync(dto.PostId, cancellationToken);

        if (postExists == null)
            return BadRequest(new { message = "존재하지 않는 게시글입니다." });

        // Validate FeatureTypeId exists
        var featureTypeExists = await _unitOfWork.Repository<FeatureType>()
            .GetByIdAsync(dto.FeatureTypeId, cancellationToken);

        if (featureTypeExists == null)
            return BadRequest(new { message = "존재하지 않는 기능 타입입니다." });

        var addFeature = new AddFeature
        {
            PostId = dto.PostId,
            FeatureTypeId = dto.FeatureTypeId,
            FeatureContent = dto.FeatureContent,
            IsDeleted = false,
            CreateDttm = DateTime.UtcNow
        };

        await _unitOfWork.Repository<AddFeature>().AddAsync(addFeature, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with FeatureType for response
        var created = await _unitOfWork.Repository<AddFeature>()
            .GetQueryable()
            .Include(af => af.FeatureType)
            .FirstOrDefaultAsync(af => af.Id == addFeature.Id, cancellationToken);

        var result = new AddFeatureDto
        {
            Id = created!.Id,
            PostId = created.PostId,
            FeatureTypeId = created.FeatureTypeId,
            FeatureContent = created.FeatureContent,
            IsDeleted = created.IsDeleted,
            CreateDttm = created.CreateDttm,
            UpdateDttm = created.UpdateDttm,
            FeatureType = created.FeatureType != null ? new FeatureTypeDto
            {
                Id = created.FeatureType.Id,
                FeatureTypeNm = created.FeatureType.FeatureTypeNm,
                FeatureTypeDesc = created.FeatureType.FeatureTypeDesc,
                IsActive = created.FeatureType.IsActive,
                DisplayOrder = created.FeatureType.DisplayOrder,
                CreateDttm = created.FeatureType.CreateDttm,
                UpdateDttm = created.FeatureType.UpdateDttm
            } : null
        };

        return CreatedAtAction(nameof(GetAddFeature), new { id = addFeature.Id }, result);
    }

    /// <summary>
    /// 추가 기능 수정
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(AddFeatureDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateAddFeature(Guid id, [FromBody] UpdateAddFeatureDto dto, CancellationToken cancellationToken)
    {
        var addFeature = await _unitOfWork.Repository<AddFeature>()
            .GetQueryable()
            .Include(af => af.FeatureType)
            .FirstOrDefaultAsync(af => af.Id == id, cancellationToken);

        if (addFeature == null)
            return NotFound(new { message = "추가 기능을 찾을 수 없습니다." });

        // Validate FeatureTypeId exists
        var featureTypeExists = await _unitOfWork.Repository<FeatureType>()
            .GetByIdAsync(dto.FeatureTypeId, cancellationToken);

        if (featureTypeExists == null)
            return BadRequest(new { message = "존재하지 않는 기능 타입입니다." });

        // Update properties
        addFeature.FeatureTypeId = dto.FeatureTypeId;
        addFeature.FeatureContent = dto.FeatureContent;
        addFeature.UpdateDttm = DateTime.UtcNow;

        await _unitOfWork.Repository<AddFeature>().UpdateAsync(addFeature, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload FeatureType if it changed
        if (addFeature.FeatureType?.Id != dto.FeatureTypeId)
        {
            addFeature = await _unitOfWork.Repository<AddFeature>()
                .GetQueryable()
                .Include(af => af.FeatureType)
                .FirstOrDefaultAsync(af => af.Id == id, cancellationToken);
        }

        var result = new AddFeatureDto
        {
            Id = addFeature!.Id,
            PostId = addFeature.PostId,
            FeatureTypeId = addFeature.FeatureTypeId,
            FeatureContent = addFeature.FeatureContent,
            IsDeleted = addFeature.IsDeleted,
            CreateDttm = addFeature.CreateDttm,
            UpdateDttm = addFeature.UpdateDttm,
            FeatureType = addFeature.FeatureType != null ? new FeatureTypeDto
            {
                Id = addFeature.FeatureType.Id,
                FeatureTypeNm = addFeature.FeatureType.FeatureTypeNm,
                FeatureTypeDesc = addFeature.FeatureType.FeatureTypeDesc,
                IsActive = addFeature.FeatureType.IsActive,
                DisplayOrder = addFeature.FeatureType.DisplayOrder,
                CreateDttm = addFeature.FeatureType.CreateDttm,
                UpdateDttm = addFeature.FeatureType.UpdateDttm
            } : null
        };

        return Ok(result);
    }

    /// <summary>
    /// 추가 기능 삭제 (Soft Delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAddFeature(Guid id, CancellationToken cancellationToken)
    {
        var addFeature = await _unitOfWork.Repository<AddFeature>()
            .GetByIdAsync(id, cancellationToken);

        if (addFeature == null)
            return NotFound(new { message = "추가 기능을 찾을 수 없습니다." });

        // Soft delete by setting IsDeleted to true
        addFeature.IsDeleted = true;
        addFeature.UpdateDttm = DateTime.UtcNow;

        await _unitOfWork.Repository<AddFeature>().UpdateAsync(addFeature, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
