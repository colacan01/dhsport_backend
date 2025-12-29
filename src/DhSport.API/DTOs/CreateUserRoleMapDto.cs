using System.ComponentModel.DataAnnotations;

namespace DhSport.API.DTOs;

public class CreateUserRoleMapDto
{
    [Required(ErrorMessage = "사용자 ID는 필수입니다.")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "역할 ID는 필수입니다.")]
    public Guid RoleId { get; set; }
}
