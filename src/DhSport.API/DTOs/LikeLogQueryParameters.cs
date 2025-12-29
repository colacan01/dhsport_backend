namespace DhSport.API.DTOs;

public class LikeLogQueryParameters
{
    public Guid? PostId { get; set; }
    public Guid? LikeUserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
