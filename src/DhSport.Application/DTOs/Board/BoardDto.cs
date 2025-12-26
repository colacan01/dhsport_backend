namespace DhSport.Application.DTOs.Board;

public class BoardDto
{
    public Guid Id { get; set; }
    public string BoardNm { get; set; } = string.Empty;
    public string? BoardDesc { get; set; }
    public Guid BoardTypeId { get; set; }
    public string BoardTypeNm { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreateDttm { get; set; }
    public int PostCount { get; set; }
}
