using DhSport.Domain.Entities.Common;
using DhSport.Domain.Entities.Content;

namespace DhSport.Domain.Entities.Features;

public class LikeLog : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid LikeUserId { get; set; }
    public DateTime CreateDttm { get; set; }

    // Navigation properties
    public virtual Post Post { get; set; } = null!;
}
