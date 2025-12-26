using DhSport.Domain.Entities.Business;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Business;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservation");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.ReservationType)
            .HasColumnName("reservation_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.ReservationDttm)
            .HasColumnName("reservation_dttm")
            .IsRequired();

        builder.Property(r => r.ReservationStatus)
            .HasColumnName("reservation_status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.CustomerNm)
            .HasColumnName("customer_nm")
            .HasMaxLength(200);

        builder.Property(r => r.CustomerTel)
            .HasColumnName("customer_tel")
            .HasMaxLength(20);

        builder.Property(r => r.CustomerEmail)
            .HasColumnName("customer_email")
            .HasMaxLength(255);

        builder.Property(r => r.ReservationNote)
            .HasColumnName("reservation_note")
            .HasMaxLength(2000);

        builder.Property(r => r.AdminNote)
            .HasColumnName("admin_note")
            .HasMaxLength(2000);

        builder.Property(r => r.IsDeleted)
            .HasColumnName("is_deleted")
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
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.ReservationDttm);
        builder.HasIndex(r => r.ReservationStatus);
    }
}
