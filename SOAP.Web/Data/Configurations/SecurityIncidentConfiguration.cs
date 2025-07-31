using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Data.Configurations
{
    public class SecurityIncidentConfiguration : IEntityTypeConfiguration<SecurityIncidentRecord>
    {
        public void Configure(EntityTypeBuilder<SecurityIncidentRecord> builder)
        {
            builder.HasKey(e => e.Id);
            
            // Security incident tracking
            builder.Property(e => e.IncidentType)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("Type of security incident (BRUTE_FORCE, DATA_BREACH, etc.)");
            
            builder.Property(e => e.Severity)
                .IsRequired()
                .HasComment("Severity level (1=Low, 2=Medium, 3=High, 4=Critical)");
            
            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(1000)
                .HasComment("Detailed description of the incident");
            
            builder.Property(e => e.AffectedUserId)
                .HasMaxLength(450)
                .HasComment("User affected by the incident");
            
            builder.Property(e => e.SourceIpAddress)
                .HasMaxLength(45)
                .HasComment("Source IP address of the incident");
            
            builder.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Open")
                .HasComment("Incident status (Open, Investigating, Resolved, Closed)");
            
            builder.Property(e => e.AutomaticResponse)
                .HasMaxLength(500)
                .HasComment("Automatic response taken by the system");
            
            builder.Property(e => e.DetectedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("When the incident was detected");
            
            builder.Property(e => e.ResolvedAt)
                .HasComment("When the incident was resolved");
            
            builder.Property(e => e.AssignedTo)
                .HasMaxLength(450)
                .HasComment("Admin user assigned to handle the incident");
            
            // Security incident management indexes
            builder.HasIndex(e => new { e.IncidentType, e.DetectedAt })
                .HasDatabaseName("IX_SecurityIncidents_Type_Date");
            
            builder.HasIndex(e => new { e.Status, e.Severity })
                .HasDatabaseName("IX_SecurityIncidents_Status_Severity");
            
            builder.HasIndex(e => new { e.Severity, e.DetectedAt })
                .HasDatabaseName("IX_SecurityIncidents_Severity_Date")
                .HasFilter("Status IN ('Open', 'Investigating')");
            
            builder.HasIndex(e => e.SourceIpAddress)
                .HasDatabaseName("IX_SecurityIncidents_SourceIP")
                .HasFilter("SourceIpAddress IS NOT NULL");
            
            builder.HasIndex(e => e.AffectedUserId)
                .HasDatabaseName("IX_SecurityIncidents_AffectedUser")
                .HasFilter("AffectedUserId IS NOT NULL");
        }
    }
}