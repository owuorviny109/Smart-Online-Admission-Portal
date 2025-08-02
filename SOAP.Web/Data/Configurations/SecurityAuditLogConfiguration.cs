using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Data.Configurations
{
    public class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
    {
        public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
        {
            builder.ToTable("SecurityAuditLogs");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.UserId)
                .HasMaxLength(450);

            builder.Property(e => e.UserRole)
                .HasMaxLength(50);

            builder.Property(e => e.IpAddress)
                .HasMaxLength(45); // IPv6 support

            builder.Property(e => e.UserAgent)
                .HasMaxLength(500);

            builder.Property(e => e.ResourceAccessed)
                .HasMaxLength(200);

            builder.Property(e => e.ActionPerformed)
                .HasMaxLength(100);

            builder.Property(e => e.FailureReason)
                .HasMaxLength(500);

            builder.Property(e => e.AdditionalData)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes for performance
            builder.HasIndex(e => new { e.EventType, e.Timestamp })
                .HasDatabaseName("IX_SecurityAuditLogs_EventType_Timestamp");

            builder.HasIndex(e => new { e.UserId, e.Timestamp })
                .HasDatabaseName("IX_SecurityAuditLogs_UserId_Timestamp");

            builder.HasIndex(e => new { e.IpAddress, e.Timestamp })
                .HasDatabaseName("IX_SecurityAuditLogs_IpAddress_Timestamp");

            builder.HasIndex(e => e.Success)
                .HasDatabaseName("IX_SecurityAuditLogs_Success");

            // Note: UserId is stored as string for flexibility (can store User.Id.ToString() or external IDs)
            // No direct foreign key relationship to User table due to type mismatch
        }
    }
}