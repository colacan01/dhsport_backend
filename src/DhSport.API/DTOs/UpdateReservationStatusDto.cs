using System.ComponentModel.DataAnnotations;
using DhSport.Domain.Enums;

namespace DhSport.API.DTOs;

public class UpdateReservationStatusDto
{
    [Required]
    public ReservationStatus ReservationStatus { get; set; }

    [MaxLength(2000)]
    public string? AdminNote { get; set; }
}
