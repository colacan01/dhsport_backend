using DhSport.Domain.Entities.Site;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Site;

public class MediaLibConfiguration : IEntityTypeConfiguration<MediaLib>
{
    public void Configure(EntityTypeBuilder<MediaLib> builder)
    {
        builder.ToTable("media_lib");

        builder.HasKey(ml => ml.Id);
        builder.Property(ml => ml.Id).HasColumnName("id");

        builder.Property(ml => ml.MediaFilePath)
            .HasColumnName("media_file_path")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(ml => ml.MediaFileNm)
            .HasColumnName("media_file_nm")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(ml => ml.MediaFileType)
            .HasColumnName("media_file_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ml => ml.MediaFileSize)
            .HasColumnName("media_file_size")
            .IsRequired();

        builder.Property(ml => ml.AltText)
            .HasColumnName("alt_text")
            .HasMaxLength(255);

        builder.Property(ml => ml.Caption)
            .HasColumnName("caption")
            .HasMaxLength(500);

        builder.Property(ml => ml.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.Property(ml => ml.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        builder.Property(ml => ml.CreateUserId)
            .HasColumnName("create_user_id");

        builder.Property(ml => ml.UpdateDttm)
            .HasColumnName("update_dttm");

        builder.Property(ml => ml.UpdateUserId)
            .HasColumnName("update_user_id");

        // Indexes
        builder.HasIndex(ml => ml.MediaFileNm);
        builder.HasIndex(ml => ml.CreateDttm);
    }
}
