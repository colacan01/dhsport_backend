using DhSport.Domain.Entities.Common;

namespace DhSport.Domain.Entities.Site;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string NotificationTitle { get; set; } = string.Empty;
    public string NotificationContent { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreateDttm { get; set; }
}
