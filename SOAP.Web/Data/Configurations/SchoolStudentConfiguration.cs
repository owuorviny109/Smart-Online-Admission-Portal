using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Data.Configurations
{
    public class SchoolStudentConfiguration : IEntityTypeConfiguration<SchoolStudent>
    {
        public void Configure(EntityTypeBuilder<SchoolStudent> builder)
        {
            builder.HasKey(ss => ss.Id);
            
            // Core properties
            builder.Property(ss => ss.KcpeIndexNumber)
                .IsRequired()
                .HasMaxLength(20)
                .HasComment("KCPE index number for student identification");
            
            builder.Property(ss => ss.StudentName)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Student full name from KCPE records");
            
            builder.Property(ss => ss.KcpeScore)
                .IsRequired()
                .HasComment("KCPE total score");
            
            builder.Property(ss => ss.Year)
                .IsRequired()
                .HasComment("KCPE examination year");
            
            builder.Property(ss => ss.PlacementStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Placed")
                .HasComment("Student placement status (Placed, NotPlaced, Transferred)");
            
            builder.Property(ss => ss.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("Record creation timestamp");
            
            // Unique constraint for KCPE number per school per year
            builder.HasIndex(ss => new { ss.KcpeIndexNumber, ss.SchoolId, ss.Year })
                .IsUnique()
                .HasDatabaseName("IX_SchoolStudents_KcpeNumber_School_Year");
            
            // Performance indexes
            builder.HasIndex(ss => new { ss.SchoolId, ss.Year })
                .HasDatabaseName("IX_SchoolStudents_School_Year");
            
            builder.HasIndex(ss => new { ss.SchoolId, ss.PlacementStatus })
                .HasDatabaseName("IX_SchoolStudents_School_Status");
            
            builder.HasIndex(ss => ss.KcpeScore)
                .HasDatabaseName("IX_SchoolStudents_Score");
            
            // Relationships
            builder.HasOne(ss => ss.School)
                .WithMany(s => s.SchoolStudents)
                .HasForeignKey(ss => ss.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Row-level security comment
            builder.HasComment("Pre-loaded student records for KCPE verification and placement tracking");
        }
    }
}