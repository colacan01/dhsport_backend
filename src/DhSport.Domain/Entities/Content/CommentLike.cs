using DhSport.Domain.Entities.Common;
using DhSport.Domain.Entities.UserManagement;

namespace DhSport.Domain.Entities.Content;

public class CommentLike : BaseEntity
{
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreateDttm { get; set; }

    // Navigation properties
    public Comment Comment { get; set; } = null!;
    public User User { get; set; } = null!;
}
