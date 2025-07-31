using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            
            // Core user properties with security constraints
            builder.Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15)
                .HasComment("Encrypted phone number for authentication");
            
            builder.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(20)
                .HasComment("User role (Parent, SchoolAdmin, SuperAdmin)");
            
            builder.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("Whether the user account is active");
            
            builder.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("Account creation timestamp");
            
            builder.Property(u => u.LastLoginAt)
                .HasComment("Last successful login timestamp");
            
            builder.Property(u => u.FailedLoginAttempts)
                .HasDefaultValue(0)
                .HasComment("Number of consecutive failed login attempts");
            
            builder.Property(u => u.LockedUntil)
                .HasComment("Account lockout expiry time");
            
            builder.Property(u => u.DeletionDate)
                .HasComment("Date when user data was anonymized/deleted");
            
            builder.Property(u => u.DeletionReason)
                .HasMaxLength(200)
                .HasComment("Reason for data deletion (GDPR compliance)");
            
            // Security indexes
            builder.HasIndex(u => u.PhoneNumber)
                .IsUnique()
                .HasDatabaseName("IX_Users_PhoneNumber");
            
            builder.HasIndex(u => new { u.Role, u.IsActive })
                .HasDatabaseName("IX_Users_Role_Active");
            
            builder.HasIndex(u => new { u.SchoolId, u.Role })
                .HasDatabaseName("IX_Users_School_Role")
                .HasFilter("SchoolId IS NOT NULL");
            
            builder.HasIndex(u => u.LockedUntil)
                .HasDatabaseName("IX_Users_LockedUntil")
                .HasFilter("LockedUntil IS NOT NULL AND LockedUntil > GETUTCDATE()");
            
            builder.HasIndex(u => new { u.IsActive, u.LastLoginAt })
                .HasDatabaseName("IX_Users_Active_LastLogin")
                .HasFilter("IsActive = 1");
            
            // Relationships with security constraints
            builder.HasOne(u => u.School)
                .WithMany()
                .HasForeignKey(u => u.SchoolId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Row-level security comment
            builder.HasComment("User accounts with role-based access control and security monitoring");
        }
    }
}