using DhSport.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Content;

public class PostFileConfiguration : IEntityTypeConfiguration<PostFile>
{
    public void Configure(EntityTypeBuilder<PostFile> builder)
    {
        builder.ToTable("post_file");

        builder.HasKey(pf => pf.Id);
        builder.Property(pf => pf.Id).HasColumnName("id");

        builder.Property(pf => pf.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(pf => pf.PostFilePath)
            .HasColumnName("post_file_path")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(pf => pf.PostFileNm)
            .HasColumnName("post_file_nm")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(pf => pf.PostFileType)
            .HasColumnName("post_file_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pf => pf.PostFileSize)
            .HasColumnName("post_file_size")
            .IsRequired();

        builder.Property(pf => pf.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        // Indexes
        builder.HasIndex(pf => pf.PostId);
    }
}
