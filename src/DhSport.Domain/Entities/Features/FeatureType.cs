using DhSport.Domain.Entities.Common;

namespace DhSport.Domain.Entities.Features;

public class FeatureType : BaseEntity, IAuditableEntity
{
    public string FeatureTypeNm { get; set; } = string.Empty;
    public string? FeatureTypeDesc { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Audit fields
    public DateTime CreateDttm { get; set; }
    public Guid? CreateUserId { get; set; }
    public DateTime? UpdateDttm { get; set; }
    public Guid? UpdateUserId { get; set; }

    // Navigation properties
    public virtual ICollection<AddFeature> Features { get; set; } = new List<AddFeature>();
}
