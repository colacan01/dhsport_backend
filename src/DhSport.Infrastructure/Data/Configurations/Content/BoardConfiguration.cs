using DhSport.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Content;

public class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.ToTable("board");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id");

        builder.Property(b => b.BoardTypeId)
            .HasColumnName("board_type_id")
            .IsRequired();

        builder.Property(b => b.BoardNm)
            .HasColumnName("board_nm")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.BoardDesc)
            .HasColumnName("board_desc")
            .HasMaxLength(1000);

        builder.Property(b => b.BoardSlug)
            .HasColumnName("board_slug")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.AllowComment)
            .HasColumnName("allow_comment")
            .IsRequired();

        builder.Property(b => b.AllowAnonymous)
            .HasColumnName("allow_anonymous")
            .IsRequired();

        builder.Property(b => b.RequireApproval)
            .HasColumnName("require_approval")
            .IsRequired();

        builder.Property(b => b.PostsPerPage)
            .HasColumnName("posts_per_page")
            .IsRequired();

        builder.Property(b => b.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(b => b.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        builder.Property(b => b.CreateUserId)
            .HasColumnName("create_user_id");

        builder.Property(b => b.UpdateDttm)
            .HasColumnName("update_dttm");

        builder.Property(b => b.UpdateUserId)
            .HasColumnName("update_user_id");

        // Indexes
        builder.HasIndex(b => b.BoardSlug).IsUnique();

        // Relationships
        builder.HasMany(b => b.Posts)
            .WithOne(p => p.Board)
            .HasForeignKey(p => p.BoardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
