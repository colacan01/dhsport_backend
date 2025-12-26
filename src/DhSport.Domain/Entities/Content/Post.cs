using DhSport.Domain.Entities.Common;
using DhSport.Domain.Entities.Features;

namespace DhSport.Domain.Entities.Content;

public class Post : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public Guid BoardId { get; set; }
    public Guid PostTypeId { get; set; }
    public Guid AuthorId { get; set; } // Changed from PostUserId for consistency
    public string Title { get; set; } = string.Empty; // Added Title
    public object? PostContent { get; set; } // JSONB content
    public string PostSlug { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDesc { get; set; }
    public string? MetaKeywords { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsNotice { get; set; } = false;
    public bool IsSecret { get; set; } = false; // Added IsSecret
    public bool IsPublished { get; set; } = false;
    public DateTime? PublishDttm { get; set; }
    public int ViewCnt { get; set; } = 0; // Added view count
    public int LikeCnt { get; set; } = 0; // Added like count
    public int CommentCnt { get; set; } = 0; // Added comment count

    // Audit fields
    public DateTime CreateDttm { get; set; }
    public Guid? CreateUserId { get; set; }
    public DateTime? UpdateDttm { get; set; }
    public Guid? UpdateUserId { get; set; }

    // Navigation properties
    public virtual Board Board { get; set; } = null!;
    public virtual PostType PostType { get; set; } = null!;
    public virtual ICollection<PostFile> Files { get; set; } = new List<PostFile>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<PostRevision> Revisions { get; set; } = new List<PostRevision>();
    public virtual ICollection<LikeLog> Likes { get; set; } = new List<LikeLog>();
    public virtual ICollection<ReadLog> Reads { get; set; } = new List<ReadLog>();
    public virtual ICollection<AddFeature> Features { get; set; } = new List<AddFeature>();
    public virtual ICollection<RelatedPost> RelatedPostsFrom { get; set; } = new List<RelatedPost>();
    public virtual ICollection<RelatedPost> RelatedPostsTo { get; set; } = new List<RelatedPost>();
}
