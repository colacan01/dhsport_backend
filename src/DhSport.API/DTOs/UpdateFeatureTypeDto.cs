using System.ComponentModel.DataAnnotations;

namespace DhSport.API.DTOs;

public class UpdateFeatureTypeDto
{
    [Required]
    [MaxLength(100)]
    public string FeatureTypeNm { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? FeatureTypeDesc { get; set; }

    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}
