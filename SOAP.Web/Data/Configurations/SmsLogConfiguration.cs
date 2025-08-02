using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Data.Configurations
{
    public class SmsLogConfiguration : IEntityTypeConfiguration<SmsLog>
    {
        public void Configure(EntityTypeBuilder<SmsLog> builder)
        {
            builder.HasKey(s => s.Id);
            
            // SMS details
            builder.Property(s => s.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15)
                .HasComment("Recipient phone number");
            
            builder.Property(s => s.Message)
                .IsRequired()
                .HasMaxLength(1000)
                .HasComment("SMS message content");
            
            builder.Property(s => s.MessageType)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("Type of SMS (OTP, NOTIFICATION, ALERT)");
            
            builder.Property(s => s.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Pending")
                .HasComment("SMS delivery status (Pending, Sent, Failed, Delivered)");
            
            // Security and tracking
            builder.Property(s => s.SentAt)
                .HasComment("When SMS was sent to provider");
            
            builder.Property(s => s.DeliveredAt)
                .HasComment("When SMS was delivered to recipient");
            
            builder.Property(s => s.FailureReason)
                .HasMaxLength(200)
                .HasComment("Reason for SMS failure");
            
            builder.Property(s => s.ProviderId)
                .HasMaxLength(100)
                .HasComment("SMS provider message ID");
            
            builder.Property(s => s.Cost)
                .HasColumnType("decimal(10,4)")
                .HasComment("Cost of SMS in local currency");
            
            // Rate limiting and abuse prevention
            builder.Property(s => s.RetryCount)
                .HasDefaultValue(0)
                .HasComment("Number of retry attempts");
            
            builder.Property(s => s.MaxRetries)
                .HasDefaultValue(3)
                .HasComment("Maximum retry attempts allowed");
            
            builder.Property(s => s.NextRetryAt)
                .HasComment("When next retry should be attempted");
            
            // Audit timestamps
            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("When SMS was queued");
            
            builder.Property(s => s.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("Last update timestamp");
            
            // Security indexes for monitoring
            builder.HasIndex(s => new { s.PhoneNumber, s.CreatedAt })
                .HasDatabaseName("IX_SmsLogs_Phone_Date");
            
            builder.HasIndex(s => s.Status)
                .HasDatabaseName("IX_SmsLogs_Status");
            
            builder.HasIndex(s => new { s.MessageType, s.Status, s.CreatedAt })
                .HasDatabaseName("IX_SmsLogs_Type_Status_Date");
            
            // Rate limiting monitoring
            builder.HasIndex(s => new { s.PhoneNumber, s.MessageType, s.CreatedAt })
                .HasDatabaseName("IX_SmsLogs_Phone_Type_Date");
            
            // Retry processing index
            builder.HasIndex(s => new { s.Status, s.NextRetryAt })
                .HasDatabaseName("IX_SmsLogs_Status_NextRetry")
                .HasFilter("Status = 'Failed' AND NextRetryAt IS NOT NULL");
            
            // Cost tracking index
            builder.HasIndex(s => new { s.Status, s.CreatedAt, s.Cost })
                .HasDatabaseName("IX_SmsLogs_Status_Date_Cost")
                .HasFilter("Status = 'Delivered' AND Cost IS NOT NULL");
            
            // Relationships
            builder.HasOne(s => s.Application)
                .WithMany()
                .HasForeignKey(s => s.ApplicationId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Row-level security comment
            builder.HasComment("SMS communication log with delivery tracking and rate limiting");
        }
    }
}