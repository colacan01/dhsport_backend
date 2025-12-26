using DhSport.Domain.Entities.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Features;

public class AddFeatureConfiguration : IEntityTypeConfiguration<AddFeature>
{
    public void Configure(EntityTypeBuilder<AddFeature> builder)
    {
        builder.ToTable("add_feature");

        builder.HasKey(af => af.Id);
        builder.Property(af => af.Id).HasColumnName("id");

        builder.Property(af => af.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(af => af.FeatureTypeId)
            .HasColumnName("feature_type_id")
            .IsRequired();

        builder.Property(af => af.FeatureContent)
            .HasColumnName("feature_content")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(af => af.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.Property(af => af.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        builder.Property(af => af.CreateUserId)
            .HasColumnName("create_user_id");

        builder.Property(af => af.UpdateDttm)
            .HasColumnName("update_dttm");

        builder.Property(af => af.UpdateUserId)
            .HasColumnName("update_user_id");

        // Indexes
        builder.HasIndex(af => af.PostId);
        builder.HasIndex(af => af.FeatureTypeId);
    }
}
