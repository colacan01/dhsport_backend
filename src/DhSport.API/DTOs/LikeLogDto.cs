namespace DhSport.API.DTOs;

public class LikeLogDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid LikeUserId { get; set; }
    public DateTime CreateDttm { get; set; }
}
