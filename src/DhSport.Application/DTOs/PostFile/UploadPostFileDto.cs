namespace DhSport.Application.DTOs.PostFile;

public class UploadPostFileDto
{
    public Guid PostId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
