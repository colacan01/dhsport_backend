using DhSport.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations;

public class CommentLikeConfiguration : IEntityTypeConfiguration<CommentLike>
{
    public void Configure(EntityTypeBuilder<CommentLike> builder)
    {
        builder.ToTable("comment_like");

        builder.HasKey(cl => cl.Id);

        builder.Property(cl => cl.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(cl => cl.CommentId)
            .HasColumnName("comment_id")
            .IsRequired();

        builder.Property(cl => cl.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(cl => cl.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        // Relationships
        builder.HasOne(cl => cl.Comment)
            .WithMany()
            .HasForeignKey(cl => cl.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cl => cl.User)
            .WithMany()
            .HasForeignKey(cl => cl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint to prevent duplicate likes
        builder.HasIndex(cl => new { cl.CommentId, cl.UserId })
            .IsUnique();
    }
}
