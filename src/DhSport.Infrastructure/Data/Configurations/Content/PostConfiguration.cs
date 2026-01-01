using DhSport.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Content;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("post");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.BoardId)
            .HasColumnName("board_id")
            .IsRequired();

        builder.Property(p => p.PostTypeId)
            .HasColumnName("post_type_id")
            .IsRequired();

        builder.Property(p => p.AuthorId)
            .HasColumnName("author_id")
            .IsRequired();

        builder.Property(p => p.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.PostContent)
            .HasColumnName("post_content")
            .HasColumnType("jsonb");

        builder.Property(p => p.PostSlug)
            .HasColumnName("post_slug")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.MetaTitle)
            .HasColumnName("meta_title")
            .HasMaxLength(200);

        builder.Property(p => p.MetaDesc)
            .HasColumnName("meta_desc")
            .HasMaxLength(500);

        builder.Property(p => p.MetaKeywords)
            .HasColumnName("meta_keywords")
            .HasMaxLength(500);

        builder.Property(p => p.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.Property(p => p.IsNotice)
            .HasColumnName("is_notice")
            .IsRequired();

        builder.Property(p => p.IsSecret)
            .HasColumnName("is_secret")
            .IsRequired();

        builder.Property(p => p.IsPublished)
            .HasColumnName("is_published")
            .IsRequired();

        builder.Property(p => p.ViewCnt)
            .HasColumnName("view_cnt")
            .IsRequired();

        builder.Property(p => p.LikeCnt)
            .HasColumnName("like_cnt")
            .IsRequired();

        builder.Property(p => p.CommentCnt)
            .HasColumnName("comment_cnt")
            .IsRequired();

        builder.Property(p => p.PublishDttm)
            .HasColumnName("publish_dttm");

        builder.Property(p => p.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        builder.Property(p => p.CreateUserId)
            .HasColumnName("create_user_id");

        builder.Property(p => p.UpdateDttm)
            .HasColumnName("update_dttm");

        builder.Property(p => p.UpdateUserId)
            .HasColumnName("update_user_id");

        // Indexes
        builder.HasIndex(p => p.PostSlug);
        builder.HasIndex(p => p.BoardId);
        builder.HasIndex(p => p.PostTypeId);
        builder.HasIndex(p => p.IsPublished);
        builder.HasIndex(p => p.PublishDttm);

        // Relationships
        builder.HasOne(p => p.Author)
            .WithMany()
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Files)
            .WithOne(f => f.Post)
            .HasForeignKey(f => f.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Revisions)
            .WithOne(r => r.Post)
            .HasForeignKey(r => r.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Likes)
            .WithOne(l => l.Post)
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Reads)
            .WithOne(r => r.Post)
            .HasForeignKey(r => r.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Features)
            .WithOne(f => f.Post)
            .HasForeignKey(f => f.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.RelatedPostsFrom)
            .WithOne(rp => rp.Post)
            .HasForeignKey(rp => rp.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.RelatedPostsTo)
            .WithOne(rp => rp.RelatedToPost)
            .HasForeignKey(rp => rp.RelatedPostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
