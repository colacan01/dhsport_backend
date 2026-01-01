using DhSport.Domain.Entities.Site;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Site;

/// <summary>
/// SiteLog 엔티티 구성
/// </summary>
public class SiteLogConfiguration : IEntityTypeConfiguration<SiteLog>
{
    public void Configure(EntityTypeBuilder<SiteLog> builder)
    {
        builder.ToTable("site_log");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("log_id");

        builder.Property(s => s.SessionId)
            .HasColumnName("session_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.UserId)
            .HasColumnName("user_id");

        builder.Property(s => s.PrevUrl)
            .HasColumnName("prev_url")
            .HasMaxLength(1000);

        builder.Property(s => s.PrevUrlId)
            .HasColumnName("prev_url_id")
            .HasMaxLength(200);

        builder.Property(s => s.CurrUrl)
            .HasColumnName("curr_url")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(s => s.CurrUrlId)
            .HasColumnName("curr_url_ud")
            .HasMaxLength(200);

        builder.Property(s => s.CurrTimestamp)
            .HasColumnName("curr_timestamp")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd()
            .IsRequired();

        // Indexes
        builder.HasIndex(s => s.SessionId)
            .HasDatabaseName("ix_site_log_session_id");

        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("ix_site_log_user_id");

        builder.HasIndex(s => s.CurrTimestamp)
            .HasDatabaseName("ix_site_log_curr_timestamp");
    }
}
