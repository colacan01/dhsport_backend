using DhSport.Domain.Entities.Common;

namespace DhSport.Domain.Entities.Content;

public class PostFile : BaseEntity
{
    public Guid PostId { get; set; }
    public string PostFilePath { get; set; } = string.Empty;
    public string PostFileNm { get; set; } = string.Empty;
    public string PostFileType { get; set; } = string.Empty; // MIME type
    public float PostFileSize { get; set; } // bytes
    public int DisplayOrder { get; set; } = 0; // Added display order
    public DateTime CreateDttm { get; set; }

    // Navigation properties
    public virtual Post Post { get; set; } = null!;
}
