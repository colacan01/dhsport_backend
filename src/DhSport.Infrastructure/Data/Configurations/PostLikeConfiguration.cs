using DhSport.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations;

public class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
{
    public void Configure(EntityTypeBuilder<PostLike> builder)
    {
        builder.ToTable("post_like");

        builder.HasKey(pl => pl.Id);

        builder.Property(pl => pl.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(pl => pl.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(pl => pl.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(pl => pl.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        // Relationships
        builder.HasOne(pl => pl.Post)
            .WithMany()
            .HasForeignKey(pl => pl.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pl => pl.User)
            .WithMany()
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint to prevent duplicate likes
        builder.HasIndex(pl => new { pl.PostId, pl.UserId })
            .IsUnique();
    }
}
