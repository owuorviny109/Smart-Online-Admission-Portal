using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Data.Configurations
{
    public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
    {
        public void Configure(EntityTypeBuilder<LoginAttempt> builder)
        {
            builder.HasKey(e => e.Id);
            
            // Login attempt tracking for security monitoring
            builder.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15)
                .HasComment("Phone number used in login attempt");
            
            builder.Property(e => e.IpAddress)
                .HasMaxLength(45) // IPv6 support
                .HasComment("IP address of login attempt");
            
            builder.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasComment("User agent of login attempt");
            
            builder.Property(e => e.Success)
                .IsRequired()
                .HasComment("Whether login attempt was successful");
            
            builder.Property(e => e.FailureReason)
                .HasMaxLength(200)
                .HasComment("Reason for login failure");
            
            builder.Property(e => e.AttemptedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("When the login attempt occurred");
            
            builder.Property(e => e.OtpAttempts)
                .HasDefaultValue(0)
                .HasComment("Number of OTP attempts for this login");
            
            builder.Property(e => e.IsBlocked)
                .HasDefaultValue(false)
                .HasComment("Whether this IP/phone is temporarily blocked");
            
            // Security monitoring indexes
            builder.HasIndex(e => new { e.PhoneNumber, e.AttemptedAt })
                .HasDatabaseName("IX_LoginAttempts_Phone_Date");
            
            builder.HasIndex(e => new { e.IpAddress, e.AttemptedAt })
                .HasDatabaseName("IX_LoginAttempts_IP_Date");
            
            builder.HasIndex(e => new { e.Success, e.AttemptedAt })
                .HasDatabaseName("IX_LoginAttempts_Success_Date");
            
            // Brute force detection index
            builder.HasIndex(e => new { e.IpAddress, e.Success, e.AttemptedAt })
                .HasDatabaseName("IX_LoginAttempts_BruteForce")
                .HasFilter("Success = 0");
            
            // Account lockout monitoring
            builder.HasIndex(e => new { e.PhoneNumber, e.Success, e.AttemptedAt })
                .HasDatabaseName("IX_LoginAttempts_AccountLockout")
                .HasFilter("Success = 0");
        }
    }
}