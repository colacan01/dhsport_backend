using System.ComponentModel.DataAnnotations;

namespace DhSport.API.DTOs;

public class CreateNotificationDto
{
    [Required(ErrorMessage = "사용자 ID는 필수입니다.")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "알림 타입은 필수입니다.")]
    [MaxLength(50)]
    public string NotificationType { get; set; } = string.Empty;

    [Required(ErrorMessage = "알림 제목은 필수입니다.")]
    [MaxLength(255)]
    public string NotificationTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "알림 내용은 필수입니다.")]
    [MaxLength(2000)]
    public string NotificationContent { get; set; } = string.Empty;
}
