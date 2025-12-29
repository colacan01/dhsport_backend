using DhSport.API.DTOs;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DhSport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeatureTypesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public FeatureTypesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 기능 타입 목록 조회
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<FeatureTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatureTypes(CancellationToken cancellationToken)
    {
        var featureTypes = await _unitOfWork.Repository<FeatureType>()
            .GetQueryable()
            .Where(ft => ft.IsActive)
            .OrderBy(ft => ft.DisplayOrder)
            .ThenBy(ft => ft.FeatureTypeNm)
            .ToListAsync(cancellationToken);

        var dtos = featureTypes.Select(ft => new FeatureTypeDto
        {
            Id = ft.Id,
            FeatureTypeNm = ft.FeatureTypeNm,
            FeatureTypeDesc = ft.FeatureTypeDesc,
            IsActive = ft.IsActive,
            DisplayOrder = ft.DisplayOrder,
            CreateDttm = ft.CreateDttm,
            UpdateDttm = ft.UpdateDttm
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// 기능 타입 단건 조회
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FeatureTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFeatureType(Guid id, CancellationToken cancellationToken)
    {
        var featureType = await _unitOfWork.Repository<FeatureType>()
            .GetByIdAsync(id, cancellationToken);

        if (featureType == null)
            return NotFound(new { message = "기능 타입을 찾을 수 없습니다." });

        var dto = new FeatureTypeDto
        {
            Id = featureType.Id,
            FeatureTypeNm = featureType.FeatureTypeNm,
            FeatureTypeDesc = featureType.FeatureTypeDesc,
            IsActive = featureType.IsActive,
            DisplayOrder = featureType.DisplayOrder,
            CreateDttm = featureType.CreateDttm,
            UpdateDttm = featureType.UpdateDttm
        };

        return Ok(dto);
    }

    /// <summary>
    /// 기능 타입 생성
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(FeatureTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateFeatureType([FromBody] CreateFeatureTypeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var featureType = new FeatureType
            {
                FeatureTypeNm = dto.FeatureTypeNm,
                FeatureTypeDesc = dto.FeatureTypeDesc,
                DisplayOrder = dto.DisplayOrder,
                IsActive = true,
                CreateDttm = DateTime.UtcNow
            };

            await _unitOfWork.Repository<FeatureType>().AddAsync(featureType, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var result = new FeatureTypeDto
            {
                Id = featureType.Id,
                FeatureTypeNm = featureType.FeatureTypeNm,
                FeatureTypeDesc = featureType.FeatureTypeDesc,
                IsActive = featureType.IsActive,
                DisplayOrder = featureType.DisplayOrder,
                CreateDttm = featureType.CreateDttm,
                UpdateDttm = featureType.UpdateDttm
            };

            return CreatedAtAction(nameof(GetFeatureType), new { id = featureType.Id }, result);
        }
        catch (DbUpdateException ex)
        {
            // Handle unique constraint violation
            if (ex.InnerException?.Message.Contains("duplicate key") == true ||
                ex.InnerException?.Message.Contains("unique constraint") == true)
            {
                return BadRequest(new { message = "이미 존재하는 기능 타입 이름입니다." });
            }
            throw;
        }
    }

    /// <summary>
    /// 기능 타입 수정
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(FeatureTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateFeatureType(Guid id, [FromBody] UpdateFeatureTypeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var featureType = await _unitOfWork.Repository<FeatureType>()
                .GetByIdAsync(id, cancellationToken);

            if (featureType == null)
                return NotFound(new { message = "기능 타입을 찾을 수 없습니다." });

            // Update properties
            featureType.FeatureTypeNm = dto.FeatureTypeNm;
            featureType.FeatureTypeDesc = dto.FeatureTypeDesc;
            featureType.IsActive = dto.IsActive;
            featureType.DisplayOrder = dto.DisplayOrder;
            featureType.UpdateDttm = DateTime.UtcNow;

            await _unitOfWork.Repository<FeatureType>().UpdateAsync(featureType, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var result = new FeatureTypeDto
            {
                Id = featureType.Id,
                FeatureTypeNm = featureType.FeatureTypeNm,
                FeatureTypeDesc = featureType.FeatureTypeDesc,
                IsActive = featureType.IsActive,
                DisplayOrder = featureType.DisplayOrder,
                CreateDttm = featureType.CreateDttm,
                UpdateDttm = featureType.UpdateDttm
            };

            return Ok(result);
        }
        catch (DbUpdateException ex)
        {
            // Handle unique constraint violation
            if (ex.InnerException?.Message.Contains("duplicate key") == true ||
                ex.InnerException?.Message.Contains("unique constraint") == true)
            {
                return BadRequest(new { message = "이미 존재하는 기능 타입 이름입니다." });
            }
            throw;
        }
    }

    /// <summary>
    /// 기능 타입 삭제 (Soft Delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteFeatureType(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var featureType = await _unitOfWork.Repository<FeatureType>()
                .GetByIdAsync(id, cancellationToken);

            if (featureType == null)
                return NotFound(new { message = "기능 타입을 찾을 수 없습니다." });

            // Soft delete by setting IsActive to false
            featureType.IsActive = false;
            featureType.UpdateDttm = DateTime.UtcNow;

            await _unitOfWork.Repository<FeatureType>().UpdateAsync(featureType, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            // Handle foreign key constraint (RESTRICT delete behavior)
            if (ex.InnerException?.Message.Contains("foreign key constraint") == true ||
                ex.InnerException?.Message.Contains("violates foreign key") == true)
            {
                return BadRequest(new { message = "이 기능 타입을 사용하는 추가 기능이 있어 삭제할 수 없습니다." });
            }
            throw;
        }
    }
}
