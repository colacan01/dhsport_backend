using System.ComponentModel.DataAnnotations;

namespace DhSport.API.DTOs;

public class UpdateMediaLibDto
{
    [MaxLength(255)]
    public string? AltText { get; set; }

    [MaxLength(500)]
    public string? Caption { get; set; }
}
