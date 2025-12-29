using System.ComponentModel.DataAnnotations;

namespace DhSport.API.DTOs;

public class UpdateMenuDto
{
    public Guid? ParentMenuId { get; set; }

    [Required(ErrorMessage = "메뉴명은 필수입니다.")]
    [MaxLength(200)]
    public string MenuNm { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? MenuUrl { get; set; }

    [MaxLength(100)]
    public string? MenuIcon { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}
