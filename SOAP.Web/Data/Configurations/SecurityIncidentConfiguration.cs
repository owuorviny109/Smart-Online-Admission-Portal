using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Data.Configurations
{
    public class SecurityIncidentConfiguration : IEntityTypeConfiguration<SecurityIncidentRecord>
    {
        public void Configure(EntityTypeBuilder<SecurityIncidentRecord> builder)
        {
            builder.ToTable("SecurityIncidents");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.IncidentType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Severity)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(e => e.AffectedUserId)
                .HasMaxLength(450);

            builder.Property(e => e.SourceIpAddress)
                .HasMaxLength(45);

            builder.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Open");

            builder.Property(e => e.AutomaticResponse)
                .HasMaxLength(500);

            builder.Property(e => e.ManualResponse)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.DetectedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.ResolvedBy)
                .HasMaxLength(450);

            // Indexes for security monitoring and reporting
            builder.HasIndex(e => new { e.IncidentType, e.DetectedAt })
                .HasDatabaseName("IX_SecurityIncidents_Type_Date");

            builder.HasIndex(e => new { e.Status, e.Severity })
                .HasDatabaseName("IX_SecurityIncidents_Status_Severity");

            builder.HasIndex(e => new { e.SourceIpAddress, e.DetectedAt })
                .HasDatabaseName("IX_SecurityIncidents_IP_Date");

            builder.HasIndex(e => e.AffectedUserId)
                .HasDatabaseName("IX_SecurityIncidents_AffectedUser");

            // Note: AffectedUserId and ResolvedBy are stored as strings for flexibility
            // No direct foreign key relationships to User table due to type mismatch
        }
    }
}