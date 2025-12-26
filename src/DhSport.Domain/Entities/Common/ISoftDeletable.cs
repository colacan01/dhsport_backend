namespace DhSport.Domain.Entities.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}
