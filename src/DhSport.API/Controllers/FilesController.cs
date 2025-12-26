using DhSport.Application.DTOs.PostFile;
using DhSport.Application.Features.PostFiles.Commands.UploadPostFile;
using DhSport.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DhSport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileService _fileService;

    public FilesController(IMediator mediator, IFileService fileService)
    {
        _mediator = mediator;
        _fileService = fileService;
    }

    /// <summary>
    /// 게시물에 파일 업로드
    /// </summary>
    [HttpPost("posts/{postId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadPostFiles(Guid postId, List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { message = "No files provided" });

        try
        {
            // Upload files using FileService and create DTOs
            var uploadedFileDtos = new List<UploadPostFileDto>();
            foreach (var file in files)
            {
                var (filePath, fileName, fileSize) = await _fileService.UploadFileAsync(file, $"posts/{postId}", cancellationToken);
                uploadedFileDtos.Add(new UploadPostFileDto
                {
                    PostId = postId,
                    FilePath = filePath,
                    FileName = fileName,
                    ContentType = file.ContentType,
                    FileSize = fileSize
                });
            }

            // Save to database using CQRS
            var uploadedFiles = await _mediator.Send(new UploadPostFileCommand(postId, uploadedFileDtos), cancellationToken);
            return Ok(uploadedFiles);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 파일 다운로드
    /// </summary>
    [HttpGet("{*filePath}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFile(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            if (!_fileService.FileExists(filePath))
                return NotFound(new { message = "File not found" });

            var stream = await _fileService.GetFileStreamAsync(filePath, cancellationToken);
            var fileName = Path.GetFileName(filePath);

            return File(stream, "application/octet-stream", fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = "File not found" });
        }
    }
}
