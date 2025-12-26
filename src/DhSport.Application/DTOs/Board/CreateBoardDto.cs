namespace DhSport.Application.DTOs.Board;

public class CreateBoardDto
{
    public string BoardNm { get; set; } = string.Empty;
    public string? BoardDesc { get; set; }
    public Guid BoardTypeId { get; set; }
    public int DisplayOrder { get; set; } = 0;
}
