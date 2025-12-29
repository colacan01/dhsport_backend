using System.ComponentModel.DataAnnotations;

namespace DhSport.API.DTOs;

public class ReadLogQueryParameters
{
    [Required(ErrorMessage = "게시글 ID는 필수입니다.")]
    public Guid PostId { get; set; }

    public Guid? ReadUserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}
