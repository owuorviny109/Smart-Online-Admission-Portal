using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Data.Configurations
{
    public class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
    {
        public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
        {
            builder.HasKey(e => e.Id);
            
            // Required fields with security-focused constraints
            builder.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("Type of security event (LOGIN, LOGOUT, DATA_ACCESS, etc.)");
            
            builder.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasComment("User identifier for the event");
            
            builder.Property(e => e.UserRole)
                .HasMaxLength(50)
                .HasComment("User role at time of event");
            
            builder.Property(e => e.IpAddress)
                .HasMaxLength(45) // IPv6 support
                .HasComment("Client IP address");
            
            builder.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasComment("Client user agent string");
            
            builder.Property(e => e.ResourceAccessed)
                .HasMaxLength(200)
                .HasComment("Resource or endpoint accessed");
            
            builder.Property(e => e.ActionPerformed)
                .HasMaxLength(100)
                .HasComment("Action performed on the resource");
            
            builder.Property(e => e.Success)
                .IsRequired()
                .HasComment("Whether the action was successful");
            
            builder.Property(e => e.FailureReason)
                .HasMaxLength(500)
                .HasComment("Reason for failure if applicable");
            
            builder.Property(e => e.AdditionalData)
                .HasColumnType("nvarchar(max)")
                .HasComment("JSON data with additional context");
            
            builder.Property(e => e.Timestamp)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("UTC timestamp of the event");
            
            // Security-focused indexes for fast querying
            builder.HasIndex(e => new { e.EventType, e.Timestamp })
                .HasDatabaseName("IX_SecurityAuditLogs_EventType_Timestamp")
                .HasFilter("EventType IS NOT NULL");
            
            builder.HasIndex(e => new { e.UserId, e.Timestamp })
                .HasDatabaseName("IX_SecurityAuditLogs_UserId_Timestamp")
                .HasFilter("UserId IS NOT NULL");
            
            builder.HasIndex(e => new { e.IpAddress, e.Timestamp })
                .HasDatabaseName("IX_SecurityAuditLogs_IpAddress_Timestamp")
                .HasFilter("IpAddress IS NOT NULL");
            
            builder.HasIndex(e => new { e.Success, e.EventType, e.Timestamp })
                .HasDatabaseName("IX_SecurityAuditLogs_Success_EventType_Timestamp");
            
            // Index for security monitoring queries
            builder.HasIndex(e => new { e.EventType, e.Success, e.IpAddress, e.Timestamp })
                .HasDatabaseName("IX_SecurityAuditLogs_Monitoring")
                .HasFilter("EventType IN ('LOGIN_FAILED', 'UNAUTHORIZED_ACCESS', 'SUSPICIOUS_ACTIVITY')");
        }
    }
}