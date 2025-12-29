namespace DhSport.API.DTOs;

public class MediaLibDto
{
    public Guid Id { get; set; }
    public string MediaFilePath { get; set; } = string.Empty;
    public string MediaFileNm { get; set; } = string.Empty;
    public string MediaFileType { get; set; } = string.Empty;
    public float MediaFileSize { get; set; }
    public string? AltText { get; set; }
    public string? Caption { get; set; }
    public DateTime CreateDttm { get; set; }
    public DateTime? UpdateDttm { get; set; }

    // 다운로드 URL (컨트롤러에서 생성)
    public string? DownloadUrl { get; set; }
}
