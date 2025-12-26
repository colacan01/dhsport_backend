using DhSport.Domain.Entities.Common;

namespace DhSport.Domain.Entities.Site;

public class MediaLib : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string MediaFilePath { get; set; } = string.Empty;
    public string MediaFileNm { get; set; } = string.Empty;
    public string MediaFileType { get; set; } = string.Empty; // MIME type
    public float MediaFileSize { get; set; } // bytes
    public string? AltText { get; set; }
    public string? Caption { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Audit fields
    public DateTime CreateDttm { get; set; }
    public Guid? CreateUserId { get; set; }
    public DateTime? UpdateDttm { get; set; }
    public Guid? UpdateUserId { get; set; }
}
