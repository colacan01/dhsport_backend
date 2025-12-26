using DhSport.Domain.Entities.Common;
using DhSport.Domain.Entities.Content;

namespace DhSport.Domain.Entities.Features;

public class AddFeature : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid PostId { get; set; }
    public Guid FeatureTypeId { get; set; }
    public string FeatureContent { get; set; } = string.Empty; // JSON stored as string
    public bool IsDeleted { get; set; } = false;

    // Audit fields
    public DateTime CreateDttm { get; set; }
    public Guid? CreateUserId { get; set; }
    public DateTime? UpdateDttm { get; set; }
    public Guid? UpdateUserId { get; set; }

    // Navigation properties
    public virtual Post Post { get; set; } = null!;
    public virtual FeatureType FeatureType { get; set; } = null!;
}
