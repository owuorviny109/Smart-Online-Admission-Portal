using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Data.Configurations
{
    public class DataProcessingConsentConfiguration : IEntityTypeConfiguration<DataProcessingConsent>
    {
        public void Configure(EntityTypeBuilder<DataProcessingConsent> builder)
        {
            builder.HasKey(e => e.Id);
            
            // GDPR/Kenya Data Protection Act compliance fields
            builder.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450)
                .HasComment("User who gave consent");
            
            builder.Property(e => e.ConsentType)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("Type of consent (DATA_PROCESSING, MARKETING, etc.)");
            
            builder.Property(e => e.Purpose)
                .IsRequired()
                .HasMaxLength(200)
                .HasComment("Purpose for which consent was given");
            
            builder.Property(e => e.ConsentGiven)
                .IsRequired()
                .HasComment("Whether consent was given or withdrawn");
            
            builder.Property(e => e.ConsentVersion)
                .IsRequired()
                .HasMaxLength(10)
                .HasComment("Version of consent terms");
            
            builder.Property(e => e.ConsentDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("When consent was given/withdrawn");
            
            builder.Property(e => e.ExpiryDate)
                .HasComment("When consent expires (if applicable)");
            
            builder.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasComment("IP address when consent was given");
            
            builder.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasComment("User agent when consent was given");
            
            // Indexes for compliance reporting
            builder.HasIndex(e => new { e.UserId, e.ConsentType })
                .HasDatabaseName("IX_DataProcessingConsents_User_Type");
            
            builder.HasIndex(e => new { e.ConsentType, e.ConsentGiven, e.ConsentDate })
                .HasDatabaseName("IX_DataProcessingConsents_Type_Given_Date");
            
            builder.HasIndex(e => e.ExpiryDate)
                .HasDatabaseName("IX_DataProcessingConsents_Expiry")
                .HasFilter("ExpiryDate IS NOT NULL");
        }
    }
}