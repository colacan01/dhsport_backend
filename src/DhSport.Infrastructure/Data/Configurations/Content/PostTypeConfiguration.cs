using DhSport.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Content;

public class PostTypeConfiguration : IEntityTypeConfiguration<PostType>
{
    public void Configure(EntityTypeBuilder<PostType> builder)
    {
        builder.ToTable("post_type");

        builder.HasKey(pt => pt.Id);
        builder.Property(pt => pt.Id).HasColumnName("id");

        builder.Property(pt => pt.PostTypeNm)
            .HasColumnName("post_type_nm")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pt => pt.PostTypeDesc)
            .HasColumnName("post_type_desc")
            .HasMaxLength(500);

        builder.Property(pt => pt.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(pt => pt.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        builder.Property(pt => pt.CreateUserId)
            .HasColumnName("create_user_id");

        builder.Property(pt => pt.UpdateDttm)
            .HasColumnName("update_dttm");

        builder.Property(pt => pt.UpdateUserId)
            .HasColumnName("update_user_id");

        // Indexes
        builder.HasIndex(pt => pt.PostTypeNm).IsUnique();

        // Relationships
        builder.HasMany(pt => pt.Posts)
            .WithOne(p => p.PostType)
            .HasForeignKey(p => p.PostTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
