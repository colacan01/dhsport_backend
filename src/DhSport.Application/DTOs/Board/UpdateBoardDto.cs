namespace DhSport.Application.DTOs.Board;

public class UpdateBoardDto
{
    public string? BoardNm { get; set; }
    public string? BoardDesc { get; set; }
    public Guid? BoardTypeId { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
}
