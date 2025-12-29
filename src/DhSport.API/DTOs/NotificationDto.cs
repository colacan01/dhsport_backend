namespace DhSport.API.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string NotificationTitle { get; set; } = string.Empty;
    public string NotificationContent { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreateDttm { get; set; }
}
