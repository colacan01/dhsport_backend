using DhSport.Domain.Entities.Common;
using DhSport.Domain.Entities.UserManagement;

namespace DhSport.Domain.Entities.Content;

public class PostLike : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreateDttm { get; set; }

    // Navigation properties
    public Post Post { get; set; } = null!;
    public User User { get; set; } = null!;
}
