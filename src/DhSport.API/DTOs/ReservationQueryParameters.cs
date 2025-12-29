using DhSport.Domain.Enums;

namespace DhSport.API.DTOs;

public class ReservationQueryParameters
{
    public Guid? UserId { get; set; }
    public ReservationType? ReservationType { get; set; }
    public ReservationStatus? ReservationStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
