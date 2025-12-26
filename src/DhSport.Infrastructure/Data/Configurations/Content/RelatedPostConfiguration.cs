using DhSport.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Content;

public class RelatedPostConfiguration : IEntityTypeConfiguration<RelatedPost>
{
    public void Configure(EntityTypeBuilder<RelatedPost> builder)
    {
        builder.ToTable("related_post");

        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id).HasColumnName("id");

        builder.Property(rp => rp.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(rp => rp.RelatedPostId)
            .HasColumnName("related_post_id")
            .IsRequired();

        builder.Property(rp => rp.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(rp => rp.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.Property(rp => rp.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        builder.Property(rp => rp.CreateUserId)
            .HasColumnName("create_user_id");

        // Composite index
        builder.HasIndex(rp => new { rp.PostId, rp.RelatedPostId }).IsUnique();
    }
}
