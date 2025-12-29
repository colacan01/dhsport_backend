using System.ComponentModel.DataAnnotations;

namespace DhSport.API.DTOs;

public class CreateReadLogDto
{
    [Required(ErrorMessage = "게시글 ID는 필수입니다.")]
    public Guid PostId { get; set; }

    // Note: ReadUserId extracted from JWT if authenticated
    // Note: IpAddress extracted from HttpContext
}
