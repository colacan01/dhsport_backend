using System.ComponentModel.DataAnnotations;

namespace DhSport.API.DTOs;

public class UploadMediaLibDto
{
    [Required(ErrorMessage = "파일은 필수입니다.")]
    public IFormFile File { get; set; } = null!;

    [MaxLength(255)]
    public string? AltText { get; set; }

    [MaxLength(500)]
    public string? Caption { get; set; }
}
