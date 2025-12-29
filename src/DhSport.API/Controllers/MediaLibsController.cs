using System.IdentityModel.Tokens.Jwt;
using DhSport.API.DTOs;
using DhSport.Application.Interfaces;
using DhSport.Domain.Entities.Site;
using DhSport.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DhSport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaLibsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;

    private static readonly string[] AllowedMimeTypes = new[]
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/svg+xml"
    };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public MediaLibsController(IUnitOfWork unitOfWork, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
    }

    /// <summary>
    /// 미디어 라이브러리 목록 조회 (공개)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MediaLibDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMediaLibs(
        [FromQuery] string? fileType,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        IQueryable<MediaLib> query = _unitOfWork.Repository<MediaLib>().GetQueryable();

        // 필터 적용
        if (!string.IsNullOrWhiteSpace(fileType))
            query = query.Where(m => m.MediaFileType.Contains(fileType));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.MediaFileNm.Contains(search) ||
                                      (m.AltText != null && m.AltText.Contains(search)));

        // 페이지네이션
        var mediaLibs = await query
            .OrderByDescending(m => m.CreateDttm)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // DownloadUrl 추가
        var dtos = mediaLibs.Select(m => new MediaLibDto
        {
            Id = m.Id,
            MediaFilePath = m.MediaFilePath,
            MediaFileNm = m.MediaFileNm,
            MediaFileType = m.MediaFileType,
            MediaFileSize = m.MediaFileSize,
            AltText = m.AltText,
            Caption = m.Caption,
            CreateDttm = m.CreateDttm,
            UpdateDttm = m.UpdateDttm,
            DownloadUrl = Url.Action(nameof(DownloadMedia), new { id = m.Id })
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// 미디어 단건 조회 (공개)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MediaLibDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMediaLib(Guid id, CancellationToken cancellationToken)
    {
        var mediaLib = await _unitOfWork.Repository<MediaLib>()
            .GetByIdAsync(id, cancellationToken);

        if (mediaLib == null)
            return NotFound(new { message = "미디어를 찾을 수 없습니다." });

        var dto = new MediaLibDto
        {
            Id = mediaLib.Id,
            MediaFilePath = mediaLib.MediaFilePath,
            MediaFileNm = mediaLib.MediaFileNm,
            MediaFileType = mediaLib.MediaFileType,
            MediaFileSize = mediaLib.MediaFileSize,
            AltText = mediaLib.AltText,
            Caption = mediaLib.Caption,
            CreateDttm = mediaLib.CreateDttm,
            UpdateDttm = mediaLib.UpdateDttm,
            DownloadUrl = Url.Action(nameof(DownloadMedia), new { id = mediaLib.Id })
        };

        return Ok(dto);
    }

    /// <summary>
    /// 파일 업로드 (관리자 전용)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "관리자")]
    [ProducesResponseType(typeof(MediaLibDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UploadMedia(
        [FromForm] UploadMediaLibDto dto,
        CancellationToken cancellationToken)
    {
        // 파일 검증
        if (!IsValidFileType(dto.File.ContentType))
            return BadRequest(new { message = "허용되지 않는 파일 타입입니다. 이미지 파일만 업로드 가능합니다." });

        if (!IsValidFileSize(dto.File.Length))
            return BadRequest(new { message = "파일 크기가 너무 큽니다. (최대 10MB)" });

        var currentUserId = GetUserIdFromToken();

        // 파일 업로드
        var (filePath, fileName, fileSize) = await _fileService.UploadFileAsync(
            dto.File, "media", cancellationToken);

        // DB 저장
        var mediaLib = new MediaLib
        {
            MediaFilePath = filePath,
            MediaFileNm = fileName,
            MediaFileType = dto.File.ContentType,
            MediaFileSize = fileSize,
            AltText = dto.AltText,
            Caption = dto.Caption,
            CreateDttm = DateTime.UtcNow,
            CreateUserId = currentUserId
        };

        await _unitOfWork.Repository<MediaLib>().AddAsync(mediaLib, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new MediaLibDto
        {
            Id = mediaLib.Id,
            MediaFilePath = mediaLib.MediaFilePath,
            MediaFileNm = mediaLib.MediaFileNm,
            MediaFileType = mediaLib.MediaFileType,
            MediaFileSize = mediaLib.MediaFileSize,
            AltText = mediaLib.AltText,
            Caption = mediaLib.Caption,
            CreateDttm = mediaLib.CreateDttm,
            UpdateDttm = mediaLib.UpdateDttm,
            DownloadUrl = Url.Action(nameof(DownloadMedia), new { id = mediaLib.Id })
        };

        return CreatedAtAction(nameof(GetMediaLib), new { id = mediaLib.Id }, result);
    }

    /// <summary>
    /// 메타데이터 수정 (관리자 전용)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "관리자")]
    [ProducesResponseType(typeof(MediaLibDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMediaLib(
        Guid id,
        [FromBody] UpdateMediaLibDto dto,
        CancellationToken cancellationToken)
    {
        var mediaLib = await _unitOfWork.Repository<MediaLib>()
            .GetByIdAsync(id, cancellationToken);

        if (mediaLib == null)
            return NotFound(new { message = "미디어를 찾을 수 없습니다." });

        var currentUserId = GetUserIdFromToken();

        // 메타데이터만 수정
        mediaLib.AltText = dto.AltText;
        mediaLib.Caption = dto.Caption;
        mediaLib.UpdateDttm = DateTime.UtcNow;
        mediaLib.UpdateUserId = currentUserId;

        await _unitOfWork.Repository<MediaLib>().UpdateAsync(mediaLib, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new MediaLibDto
        {
            Id = mediaLib.Id,
            MediaFilePath = mediaLib.MediaFilePath,
            MediaFileNm = mediaLib.MediaFileNm,
            MediaFileType = mediaLib.MediaFileType,
            MediaFileSize = mediaLib.MediaFileSize,
            AltText = mediaLib.AltText,
            Caption = mediaLib.Caption,
            CreateDttm = mediaLib.CreateDttm,
            UpdateDttm = mediaLib.UpdateDttm,
            DownloadUrl = Url.Action(nameof(DownloadMedia), new { id = mediaLib.Id })
        };

        return Ok(result);
    }

    /// <summary>
    /// 미디어 삭제 (관리자 전용, Soft Delete + 파일 삭제)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "관리자")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMediaLib(Guid id, CancellationToken cancellationToken)
    {
        var mediaLib = await _unitOfWork.Repository<MediaLib>()
            .GetByIdAsync(id, cancellationToken);

        if (mediaLib == null)
            return NotFound(new { message = "미디어를 찾을 수 없습니다." });

        var currentUserId = GetUserIdFromToken();

        // Soft delete
        mediaLib.IsDeleted = true;
        mediaLib.UpdateDttm = DateTime.UtcNow;
        mediaLib.UpdateUserId = currentUserId;

        await _unitOfWork.Repository<MediaLib>().UpdateAsync(mediaLib, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 실제 파일 삭제
        try
        {
            await _fileService.DeleteFileAsync(mediaLib.MediaFilePath, cancellationToken);
        }
        catch
        {
            // 파일 삭제 실패 시 로그만 기록하고 계속 진행
            // (DB는 이미 soft delete 되었으므로 사용자에게는 성공으로 응답)
        }

        return NoContent();
    }

    /// <summary>
    /// 파일 다운로드 (공개)
    /// </summary>
    [HttpGet("{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadMedia(Guid id, CancellationToken cancellationToken)
    {
        var mediaLib = await _unitOfWork.Repository<MediaLib>()
            .GetByIdAsync(id, cancellationToken);

        if (mediaLib == null)
            return NotFound(new { message = "미디어를 찾을 수 없습니다." });

        if (!_fileService.FileExists(mediaLib.MediaFilePath))
            return NotFound(new { message = "파일이 존재하지 않습니다." });

        var stream = await _fileService.GetFileStreamAsync(mediaLib.MediaFilePath, cancellationToken);

        return File(stream, mediaLib.MediaFileType, mediaLib.MediaFileNm);
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

    private bool IsValidFileType(string contentType)
    {
        return AllowedMimeTypes.Contains(contentType.ToLower());
    }

    private bool IsValidFileSize(long fileSize)
    {
        return fileSize > 0 && fileSize <= MaxFileSize;
    }
}
