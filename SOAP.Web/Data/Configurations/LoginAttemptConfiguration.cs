using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Data.Configurations
{
    public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
    {
        public void Configure(EntityTypeBuilder<LoginAttempt> builder)
        {
            builder.ToTable("LoginAttempts");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15);

            builder.Property(e => e.IpAddress)
                .HasMaxLength(45);

            builder.Property(e => e.UserAgent)
                .HasMaxLength(500);

            builder.Property(e => e.FailureReason)
                .HasMaxLength(200);

            builder.Property(e => e.AttemptedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.OtpCode)
                .HasMaxLength(100); // Hashed OTP

            // Indexes for security monitoring and rate limiting
            builder.HasIndex(e => new { e.PhoneNumber, e.AttemptedAt })
                .HasDatabaseName("IX_LoginAttempts_Phone_Date");

            builder.HasIndex(e => new { e.IpAddress, e.AttemptedAt })
                .HasDatabaseName("IX_LoginAttempts_IP_Date");

            builder.HasIndex(e => new { e.Success, e.AttemptedAt })
                .HasDatabaseName("IX_LoginAttempts_Success_Date");

            builder.HasIndex(e => new { e.PhoneNumber, e.Success, e.AttemptedAt })
                .HasDatabaseName("IX_LoginAttempts_Phone_Success_Date");

            // Relationships - Link to User via PhoneNumber
            // Note: This creates a relationship based on PhoneNumber matching
            // The User entity will be resolved in the service layer
        }
    }
}