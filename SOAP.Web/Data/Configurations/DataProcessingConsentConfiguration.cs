using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Data.Configurations
{
    public class DataProcessingConsentConfiguration : IEntityTypeConfiguration<DataProcessingConsent>
    {
        public void Configure(EntityTypeBuilder<DataProcessingConsent> builder)
        {
            builder.ToTable("DataProcessingConsents");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(e => e.ConsentType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Purpose)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.ConsentVersion)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(e => e.IpAddress)
                .HasMaxLength(45);

            builder.Property(e => e.UserAgent)
                .HasMaxLength(500);

            builder.Property(e => e.ConsentDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes for performance and compliance queries
            builder.HasIndex(e => new { e.UserId, e.ConsentType })
                .HasDatabaseName("IX_DataProcessingConsents_User_Type");

            builder.HasIndex(e => new { e.ConsentType, e.IsActive })
                .HasDatabaseName("IX_DataProcessingConsents_Type_Active");

            builder.HasIndex(e => e.ConsentDate)
                .HasDatabaseName("IX_DataProcessingConsents_ConsentDate");

            // Note: UserId is stored as string for flexibility (can store User.Id.ToString() or external IDs)
            // No direct foreign key relationship to User table due to type mismatch
        }
    }
}