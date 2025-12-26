using DhSport.Domain.Entities.Common;

namespace DhSport.Domain.Entities.Content;

public class Comment : BaseEntity, ISoftDeletable
{
    public Guid? ParentCommentId { get; set; }
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; } // Changed from CommentUserId for consistency
    public object CommentContent { get; set; } = new(); // JSONB content
    public int LikeCnt { get; set; } = 0; // Added like count
    public bool IsDeleted { get; set; } = false;
    public DateTime CreateDttm { get; set; }
    public DateTime? UpdateDttm { get; set; }

    // Navigation properties
    public virtual Comment? ParentComment { get; set; }
    public virtual Post Post { get; set; } = null!;
    public virtual ICollection<Comment> ChildComments { get; set; } = new List<Comment>();
}
