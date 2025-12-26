using DhSport.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Content;

public class BoardTypeConfiguration : IEntityTypeConfiguration<BoardType>
{
    public void Configure(EntityTypeBuilder<BoardType> builder)
    {
        builder.ToTable("board_type");

        builder.HasKey(bt => bt.Id);
        builder.Property(bt => bt.Id).HasColumnName("id");

        builder.Property(bt => bt.BoardTypeNm)
            .HasColumnName("board_type_nm")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(bt => bt.BoardTypeDesc)
            .HasColumnName("board_type_desc")
            .HasMaxLength(500);

        builder.Property(bt => bt.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(bt => bt.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        builder.Property(bt => bt.CreateUserId)
            .HasColumnName("create_user_id");

        builder.Property(bt => bt.UpdateDttm)
            .HasColumnName("update_dttm");

        builder.Property(bt => bt.UpdateUserId)
            .HasColumnName("update_user_id");

        // Indexes
        builder.HasIndex(bt => bt.BoardTypeNm).IsUnique();

        // Relationships
        builder.HasMany(bt => bt.Boards)
            .WithOne(b => b.BoardType)
            .HasForeignKey(b => b.BoardTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
