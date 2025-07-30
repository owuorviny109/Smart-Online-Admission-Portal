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
            
            builder.Property(a => a.KcpeIndexNumber)
                .IsRequired()
                .HasMaxLength(20);
            
            builder.Property(a => a.StudentName)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(a => a.ParentPhone)
                .IsRequired()
                .HasMaxLength(15);
            
            builder.Property(a => a.ParentName)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(a => a.EmergencyContact)
                .IsRequired()
                .HasMaxLength(15);
            
            builder.Property(a => a.EmergencyName)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(a => a.BoardingStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Day");
            
            builder.Property(a => a.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            
            builder.Property(a => a.AdmissionCode)
                .HasMaxLength(10);
            
            builder.Property(a => a.CheckedIn)
                .HasDefaultValue(false);
            
            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
            
            builder.Property(a => a.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");
            
            // Indexes
            builder.HasIndex(a => a.KcpeIndexNumber)
                .IsUnique();
            
            builder.HasIndex(a => new { a.SchoolId, a.Status });
            
            builder.HasIndex(a => a.CreatedAt);
            
            // Relationships
            builder.HasOne(a => a.School)
                .WithMany(s => s.Applications)
                .HasForeignKey(a => a.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(a => a.Documents)
                .WithOne(d => d.Application)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}