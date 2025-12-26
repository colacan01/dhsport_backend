using DhSport.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.UserManagement;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("role");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.RoleNm)
            .HasColumnName("role_nm")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.RoleDesc)
            .HasColumnName("role_desc")
            .HasMaxLength(500);

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(r => r.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        builder.Property(r => r.CreateUserId)
            .HasColumnName("create_user_id");

        builder.Property(r => r.UpdateDttm)
            .HasColumnName("update_dttm");

        builder.Property(r => r.UpdateUserId)
            .HasColumnName("update_user_id");

        // Indexes
        builder.HasIndex(r => r.RoleNm).IsUnique();

        // Relationships
        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
