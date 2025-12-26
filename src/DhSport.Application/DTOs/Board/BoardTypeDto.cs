namespace DhSport.Application.DTOs.Board;

public class BoardTypeDto
{
    public Guid Id { get; set; }
    public string BoardTypeNm { get; set; } = string.Empty;
    public string? BoardTypeDesc { get; set; }
    public bool IsActive { get; set; }
}
