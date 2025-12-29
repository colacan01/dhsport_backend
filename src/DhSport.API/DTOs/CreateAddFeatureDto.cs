using System.ComponentModel.DataAnnotations;

namespace DhSport.API.DTOs;

public class CreateAddFeatureDto
{
    [Required]
    public Guid PostId { get; set; }

    [Required]
    public Guid FeatureTypeId { get; set; }

    [Required]
    public string FeatureContent { get; set; } = string.Empty;
}
