using DhSport.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DhSport.Infrastructure.Data.Configurations.Content;

public class PostRevisionConfiguration : IEntityTypeConfiguration<PostRevision>
{
    public void Configure(EntityTypeBuilder<PostRevision> builder)
    {
        builder.ToTable("post_revision");

        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.Id).HasColumnName("id");

        builder.Property(pr => pr.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(pr => pr.PostContent)
            .HasColumnName("post_content")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(pr => pr.RevisionNote)
            .HasColumnName("revision_note")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(pr => pr.RevisionUserId)
            .HasColumnName("revision_user_id")
            .IsRequired();

        builder.Property(pr => pr.CreateDttm)
            .HasColumnName("create_dttm")
            .IsRequired();

        // Indexes
        builder.HasIndex(pr => pr.PostId);
        builder.HasIndex(pr => pr.CreateDttm);
    }
}
