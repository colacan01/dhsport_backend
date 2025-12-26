using DhSport.Domain.Entities.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Features;

public class LikeLogConfiguration : IEntityTypeConfiguration<LikeLog>
{
    public void Configure(EntityTypeBuilder<LikeLog> builder)
    {
        builder.ToTable("like_log");

        builder.HasKey(ll => ll.Id);
        builder.Property(ll => ll.Id).HasColumnName("id");

        builder.Property(ll => ll.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(ll => ll.LikeUserId)
            .HasColumnName("like_user_id")
            .IsRequired();

        builder.Property(ll => ll.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        // Composite index to prevent duplicate likes
        builder.HasIndex(ll => new { ll.PostId, ll.LikeUserId }).IsUnique();
    }
}
