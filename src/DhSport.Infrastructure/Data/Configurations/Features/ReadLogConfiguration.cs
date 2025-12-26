using DhSport.Domain.Entities.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Features;

public class ReadLogConfiguration : IEntityTypeConfiguration<ReadLog>
{
    public void Configure(EntityTypeBuilder<ReadLog> builder)
    {
        builder.ToTable("read_log");

        builder.HasKey(rl => rl.Id);
        builder.Property(rl => rl.Id).HasColumnName("id");

        builder.Property(rl => rl.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(rl => rl.ReadUserId)
            .HasColumnName("read_user_id");

        builder.Property(rl => rl.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(rl => rl.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        // Indexes
        builder.HasIndex(rl => rl.PostId);
        builder.HasIndex(rl => rl.CreateDttm);
    }
}
