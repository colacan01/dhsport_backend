using DhSport.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.UserManagement;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");

        builder.Property(u => u.LogonId)
            .HasColumnName("logon_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Passwd)
            .HasColumnName("passwd")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.UserNm)
            .HasColumnName("user_nm")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.Tel)
            .HasColumnName("tel")
            .HasMaxLength(20);

        builder.Property(u => u.ProfileImg)
            .HasColumnName("profile_img")
            .HasMaxLength(500);

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(u => u.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.Property(u => u.LastLoginDttm)
            .HasColumnName("last_login_dttm");

        builder.Property(u => u.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        builder.Property(u => u.CreateUserId)
            .HasColumnName("create_user_id");

        builder.Property(u => u.UpdateDttm)
            .HasColumnName("update_dttm");

        builder.Property(u => u.UpdateUserId)
            .HasColumnName("update_user_id");

        // Indexes
        builder.HasIndex(u => u.LogonId).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        // Relationships
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
