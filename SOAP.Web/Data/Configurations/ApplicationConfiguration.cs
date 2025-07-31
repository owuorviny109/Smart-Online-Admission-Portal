using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Data.Configurations
{
    public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
    {
        public void Configure(EntityTypeBuilder<Application> builder)
        {
            builder.HasKey(a => a.Id);
            
            // Core application properties
            builder.Property(a => a.KcpeIndexNumber)
                .IsRequired()
                .HasMaxLength(20)
                .HasComment("KCPE index number for student verification");
            
            builder.Property(a => a.StudentName)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Student full name");
            
            // Sensitive personal data - marked for encryption
            builder.Property(a => a.ParentPhone)
                .IsRequired()
                .HasMaxLength(15)
                .HasComment("Encrypted parent phone number");
            
            builder.Property(a => a.ParentName)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Parent/guardian full name");
            
            builder.Property(a => a.EmergencyContact)
                .IsRequired()
                .HasMaxLength(15)
                .HasComment("Encrypted emergency contact phone");
            
            builder.Property(a => a.EmergencyName)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Emergency contact name");
            
            builder.Property(a => a.HomeAddress)
                .HasMaxLength(500)
                .HasComment("Encrypted home address");
            
            builder.Property(a => a.MedicalConditions)
                .HasMaxLength(1000)
                .HasComment("Encrypted medical conditions/allergies");
            
            // Application status and metadata
            builder.Property(a => a.BoardingStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Day")
                .HasComment("Boarding or Day scholar preference");
            
            builder.Property(a => a.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending")
                .HasComment("Application status (Pending, Approved, Rejected)");
            
            builder.Property(a => a.AdmissionCode)
                .HasMaxLength(10)
                .HasComment("Unique admission code for approved applications");
            
            builder.Property(a => a.CheckedIn)
                .HasDefaultValue(false)
                .HasComment("Whether student has physically checked in");
            
            // Audit timestamps
            builder.Property(a => a.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("Application creation timestamp");
            
            builder.Property(a => a.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("Last update timestamp");
            
            builder.Property(a => a.SubmittedAt)
                .HasComment("When application was submitted for review");
            
            builder.Property(a => a.ReviewedAt)
                .HasComment("When application was reviewed by admin");
            
            builder.Property(a => a.ReviewedBy)
                .HasMaxLength(450)
                .HasComment("Admin user who reviewed the application");
            
            // Security and performance indexes
            builder.HasIndex(a => a.KcpeIndexNumber)
                .IsUnique()
                .HasDatabaseName("IX_Applications_KcpeNumber");
            
            builder.HasIndex(a => new { a.SchoolId, a.Status })
                .HasDatabaseName("IX_Applications_School_Status");
            
            builder.HasIndex(a => new { a.SchoolId, a.Status, a.CreatedAt })
                .HasDatabaseName("IX_Applications_School_Status_Date");
            
            builder.HasIndex(a => new { a.Status, a.UpdatedAt })
                .HasDatabaseName("IX_Applications_Status_Updated");
            
            builder.HasIndex(a => a.CreatedAt)
                .HasDatabaseName("IX_Applications_CreatedAt");
            
            // Security monitoring indexes
            builder.HasIndex(a => new { a.ParentPhone, a.SchoolId })
                .HasDatabaseName("IX_Applications_ParentPhone_School")
                .HasFilter("ParentPhone IS NOT NULL");
            
            builder.HasIndex(a => a.ReviewedBy)
                .HasDatabaseName("IX_Applications_ReviewedBy")
                .HasFilter("ReviewedBy IS NOT NULL");
            
            // Relationships with security constraints
            builder.HasOne(a => a.School)
                .WithMany(s => s.Applications)
                .HasForeignKey(a => a.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(a => a.Documents)
                .WithOne(d => d.Application)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Row-level security comment
            builder.HasComment("Student applications with encrypted personal data and access control");
        }
    }
}