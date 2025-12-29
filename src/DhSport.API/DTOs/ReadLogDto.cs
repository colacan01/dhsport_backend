namespace DhSport.API.DTOs;

public class ReadLogDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid? ReadUserId { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreateDttm { get; set; }
}
