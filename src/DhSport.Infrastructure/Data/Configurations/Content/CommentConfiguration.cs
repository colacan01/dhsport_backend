using DhSport.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Content;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comment");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.ParentCommentId)
            .HasColumnName("parent_comment_id");

        builder.Property(c => c.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(c => c.AuthorId)
            .HasColumnName("author_id")
            .IsRequired();

        builder.Property(c => c.CommentContent)
            .HasColumnName("comment_content")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(c => c.LikeCnt)
            .HasColumnName("like_cnt")
            .IsRequired();

        builder.Property(c => c.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.Property(c => c.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        builder.Property(c => c.UpdateDttm)
            .HasColumnName("update_dttm");

        // Indexes
        builder.HasIndex(c => c.PostId);
        builder.HasIndex(c => c.ParentCommentId);

        // Self-referencing relationship
        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.ChildComments)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
