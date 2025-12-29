using DhSport.Domain.Enums;

namespace DhSport.API.DTOs;

public class ReservationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ReservationType ReservationType { get; set; }
    public DateTime ReservationDttm { get; set; }
    public ReservationStatus ReservationStatus { get; set; }
    public string? CustomerNm { get; set; }
    public string? CustomerTel { get; set; }
    public string? CustomerEmail { get; set; }
    public string? ReservationNote { get; set; }
    public string? AdminNote { get; set; }
    public DateTime CreateDttm { get; set; }
    public DateTime? UpdateDttm { get; set; }

    // Nested user information (optional)
    public UserBasicDto? User { get; set; }
}
