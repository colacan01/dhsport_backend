using DhSport.Domain.Entities.Common;
using DhSport.Domain.Entities.Content;

namespace DhSport.Domain.Entities.Features;

public class ReadLog : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid? ReadUserId { get; set; } // Nullable for anonymous views
    public string? IpAddress { get; set; }
    public DateTime CreateDttm { get; set; }

    // Navigation properties
    public virtual Post Post { get; set; } = null!;
}
