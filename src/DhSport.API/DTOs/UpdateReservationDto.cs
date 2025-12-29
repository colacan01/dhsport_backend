using System.ComponentModel.DataAnnotations;
using DhSport.Domain.Enums;

namespace DhSport.API.DTOs;

public class UpdateReservationDto
{
    [Required]
    public ReservationType ReservationType { get; set; }

    [Required]
    public DateTime ReservationDttm { get; set; }

    [MaxLength(200)]
    public string? CustomerNm { get; set; }

    [MaxLength(20)]
    public string? CustomerTel { get; set; }

    [MaxLength(255)]
    [EmailAddress]
    public string? CustomerEmail { get; set; }

    [MaxLength(2000)]
    public string? ReservationNote { get; set; }
}
