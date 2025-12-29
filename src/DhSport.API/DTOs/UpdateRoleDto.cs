using System.ComponentModel.DataAnnotations;

namespace DhSport.API.DTOs;

public class UpdateRoleDto
{
    [Required(ErrorMessage = "역할 이름은 필수입니다.")]
    [MaxLength(100, ErrorMessage = "역할 이름은 100자를 초과할 수 없습니다.")]
    public string RoleNm { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "역할 설명은 500자를 초과할 수 없습니다.")]
    public string? RoleDesc { get; set; }

    public bool IsActive { get; set; }
}
