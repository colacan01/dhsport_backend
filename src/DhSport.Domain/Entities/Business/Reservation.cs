using DhSport.Domain.Entities.Common;
using DhSport.Domain.Enums;

namespace DhSport.Domain.Entities.Business;

public class Reservation : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid UserId { get; set; }
    public string ReservationType { get; set; } = string.Empty;
    public DateTime ReservationDttm { get; set; }
    public string ReservationStatus { get; set; } = "Pending";
    public string? CustomerNm { get; set; }
    public string? CustomerTel { get; set; }
    public string? CustomerEmail { get; set; }
    public string? ReservationNote { get; set; }
    public string? AdminNote { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Audit fields
    public DateTime CreateDttm { get; set; }
    public Guid? CreateUserId { get; set; }
    public DateTime? UpdateDttm { get; set; }
    public Guid? UpdateUserId { get; set; }
}
