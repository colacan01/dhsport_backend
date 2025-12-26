using DhSport.Domain.Entities.Common;

namespace DhSport.Domain.Entities.Content;

public class PostRevision : BaseEntity
{
    public Guid PostId { get; set; }
    public string PostContent { get; set; } = string.Empty; // JSON stored as string
    public string RevisionNote { get; set; } = string.Empty;
    public Guid RevisionUserId { get; set; }
    public DateTime CreateDttm { get; set; }

    // Navigation properties
    public virtual Post Post { get; set; } = null!;
}
