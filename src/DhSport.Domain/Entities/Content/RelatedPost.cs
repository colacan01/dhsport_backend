using DhSport.Domain.Entities.Common;

namespace DhSport.Domain.Entities.Content;

public class RelatedPost : BaseEntity, ISoftDeletable
{
    public Guid PostId { get; set; }
    public Guid RelatedPostId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreateDttm { get; set; }
    public Guid? CreateUserId { get; set; }

    // Navigation properties
    public virtual Post Post { get; set; } = null!;
    public virtual Post RelatedToPost { get; set; } = null!;
}
