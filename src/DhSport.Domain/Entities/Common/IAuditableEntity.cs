namespace DhSport.Domain.Entities.Common;

public interface IAuditableEntity
{
    DateTime CreateDttm { get; set; }
    Guid? CreateUserId { get; set; }
    DateTime? UpdateDttm { get; set; }
    Guid? UpdateUserId { get; set; }
}
