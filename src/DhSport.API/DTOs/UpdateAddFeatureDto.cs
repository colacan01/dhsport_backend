using System.ComponentModel.DataAnnotations;

namespace DhSport.API.DTOs;

public class UpdateAddFeatureDto
{
    [Required]
    public Guid FeatureTypeId { get; set; }

    [Required]
    public string FeatureContent { get; set; } = string.Empty;
}
