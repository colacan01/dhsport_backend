using System.ComponentModel.DataAnnotations;

namespace DhSport.API.DTOs;

public class CreateLikeLogDto
{
    [Required(ErrorMessage = "게시글 ID는 필수입니다.")]
    public Guid PostId { get; set; }

    // Note: LikeUserId will be extracted from JWT token
}
