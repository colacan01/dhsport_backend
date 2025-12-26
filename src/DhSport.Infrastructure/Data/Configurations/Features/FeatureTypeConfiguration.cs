using DhSport.Domain.Entities.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Features;

public class FeatureTypeConfiguration : IEntityTypeConfiguration<FeatureType>
{
    public void Configure(EntityTypeBuilder<FeatureType> builder)
    {
        builder.ToTable("feature_type");

        builder.HasKey(ft => ft.Id);
        builder.Property(ft => ft.Id).HasColumnName("id");

        builder.Property(ft => ft.FeatureTypeNm)
            .HasColumnName("feature_type_nm")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ft => ft.FeatureTypeDesc)
            .HasColumnName("feature_type_desc")
            .HasMaxLength(500);

        builder.Property(ft => ft.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(ft => ft.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        builder.Property(ft => ft.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        builder.Property(ft => ft.CreateUserId)
            .HasColumnName("create_user_id");

        builder.Property(ft => ft.UpdateDttm)
            .HasColumnName("update_dttm");

        builder.Property(ft => ft.UpdateUserId)
            .HasColumnName("update_user_id");

        // Indexes
        builder.HasIndex(ft => ft.FeatureTypeNm).IsUnique();

        // Relationships
        builder.HasMany(ft => ft.Features)
            .WithOne(f => f.FeatureType)
            .HasForeignKey(f => f.FeatureTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
