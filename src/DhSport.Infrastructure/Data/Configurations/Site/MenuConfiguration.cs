using DhSport.Domain.Entities.Site;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Site;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("menu");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.ParentMenuId)
            .HasColumnName("parent_menu_id");

        builder.Property(m => m.MenuNm)
            .HasColumnName("menu_nm")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.MenuUrl)
            .HasColumnName("menu_url")
            .HasMaxLength(500);

        builder.Property(m => m.MenuIcon)
            .HasColumnName("menu_icon")
            .HasMaxLength(100);

        builder.Property(m => m.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        builder.Property(m => m.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(m => m.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        builder.Property(m => m.CreateUserId)
            .HasColumnName("create_user_id");

        builder.Property(m => m.UpdateDttm)
            .HasColumnName("update_dttm");

        builder.Property(m => m.UpdateUserId)
            .HasColumnName("update_user_id");

        // Indexes
        builder.HasIndex(m => m.ParentMenuId);
        builder.HasIndex(m => m.DisplayOrder);

        // Self-referencing relationship
        builder.HasOne(m => m.ParentMenu)
            .WithMany(m => m.ChildMenus)
            .HasForeignKey(m => m.ParentMenuId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
