using DhSport.Domain.Entities.Site;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Site;

public class SiteConfigConfiguration : IEntityTypeConfiguration<SiteConfig>
{
    public void Configure(EntityTypeBuilder<SiteConfig> builder)
    {
        builder.ToTable("site_config");

        builder.HasKey(sc => sc.Id);
        builder.Property(sc => sc.Id).HasColumnName("id");

        builder.Property(sc => sc.ConfigKey)
            .HasColumnName("config_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(sc => sc.ConfigValue)
            .HasColumnName("config_value")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(sc => sc.ConfigDesc)
            .HasColumnName("config_desc")
            .HasMaxLength(500);

        builder.Property(sc => sc.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(sc => sc.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        builder.Property(sc => sc.CreateUserId)
            .HasColumnName("create_user_id");

        builder.Property(sc => sc.UpdateDttm)
            .HasColumnName("update_dttm");

        builder.Property(sc => sc.UpdateUserId)
            .HasColumnName("update_user_id");

        // Indexes
        builder.HasIndex(sc => sc.ConfigKey).IsUnique();
    }
}
