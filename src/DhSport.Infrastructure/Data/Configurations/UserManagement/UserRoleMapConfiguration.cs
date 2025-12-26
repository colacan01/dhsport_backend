using DhSport.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.UserManagement;

public class UserRoleMapConfiguration : IEntityTypeConfiguration<UserRoleMap>
{
    public void Configure(EntityTypeBuilder<UserRoleMap> builder)
    {
        builder.ToTable("user_role_map");

        builder.HasKey(ur => ur.Id);
        builder.Property(ur => ur.Id).HasColumnName("id");

        builder.Property(ur => ur.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ur => ur.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(ur => ur.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(ur => ur.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        builder.Property(ur => ur.CreateUserId)
            .HasColumnName("create_user_id");

        // Composite index for user-role combination
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();

        // Relationships are defined in User and Role configurations
    }
}
