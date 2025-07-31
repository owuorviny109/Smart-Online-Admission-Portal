using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Data.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.HasKey(d => d.Id);
            
            // Document metadata
            builder.Property(d => d.DocumentType)
                .IsRequired()
                .HasMaxLength(50)
                .HasComment("Type of document (BIRTH_CERT, KCPE_CERT, etc.)");
            
            builder.Property(d => d.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("Original filename as uploaded by user");
            
            builder.Property(d => d.SecureFileName)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("Secure filename used for storage");
            
            builder.Property(d => d.ContentType)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("MIME type of the document");
            
            builder.Property(d => d.FileSize)
                .IsRequired()
                .HasComment("File size in bytes");
            
            builder.Property(d => d.FileHash)
                .HasMaxLength(64)
                .HasComment("SHA-256 hash of file content for integrity verification");
            
            // Security and verification status
            builder.Property(d => d.VerificationStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Pending")
                .HasComment("Document verification status (Pending, Verified, Rejected)");
            
            builder.Property(d => d.VerifiedBy)
                .HasMaxLength(450)
                .HasComment("Admin user who verified the document");
            
            builder.Property(d => d.VerifiedAt)
                .HasComment("When document was verified");
            
            builder.Property(d => d.RejectionReason)
                .HasMaxLength(500)
                .HasComment("Reason for document rejection");
            
            builder.Property(d => d.IsVirusScanPassed)
                .HasDefaultValue(false)
                .HasComment("Whether document passed virus scan");
            
            builder.Property(d => d.VirusScanDate)
                .HasComment("When virus scan was performed");
            
            // Audit timestamps
            builder.Property(d => d.UploadedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("When document was uploaded");
            
            builder.Property(d => d.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("Last update timestamp");
            
            // Access control
            builder.Property(d => d.AccessLevel)
                .HasMaxLength(20)
                .HasDefaultValue("Private")
                .HasComment("Access level (Private, SchoolAdmin, Public)");
            
            builder.Property(d => d.EncryptionKey)
                .HasMaxLength(100)
                .HasComment("Encryption key reference for sensitive documents");
            
            // Security indexes
            builder.HasIndex(d => new { d.ApplicationId, d.DocumentType })
                .HasDatabaseName("IX_Documents_Application_Type");
            
            builder.HasIndex(d => new { d.VerificationStatus, d.UploadedAt })
                .HasDatabaseName("IX_Documents_Status_Uploaded");
            
            builder.HasIndex(d => d.VerifiedBy)
                .HasDatabaseName("IX_Documents_VerifiedBy")
                .HasFilter("VerifiedBy IS NOT NULL");
            
            builder.HasIndex(d => d.FileHash)
                .HasDatabaseName("IX_Documents_FileHash")
                .HasFilter("FileHash IS NOT NULL");
            
            builder.HasIndex(d => new { d.IsVirusScanPassed, d.VirusScanDate })
                .HasDatabaseName("IX_Documents_VirusScan")
                .HasFilter("IsVirusScanPassed = 0 OR VirusScanDate IS NULL");
            
            // Security monitoring index
            builder.HasIndex(d => new { d.ApplicationId, d.AccessLevel, d.UploadedAt })
                .HasDatabaseName("IX_Documents_Application_Access_Date");
            
            // Relationships
            builder.HasOne(d => d.Application)
                .WithMany(a => a.Documents)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Row-level security comment
            builder.HasComment("Document storage with encryption, virus scanning, and access control");
        }
    }
}