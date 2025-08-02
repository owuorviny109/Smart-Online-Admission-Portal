using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [Required]
        [StringLength(50)]
        public string DocumentType { get; set; } = "";

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = "";

        [Required]
        [StringLength(255)]
        public string SecureFileName { get; set; } = "";

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = "";

        [Required]
        public long FileSize { get; set; }

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = "";

        // Security properties
        [StringLength(64)]
        public string? FileHash { get; set; }

        [StringLength(20)]
        public string VerificationStatus { get; set; } = "Pending";

        [StringLength(20)]
        public string UploadStatus { get; set; } = "Pending";

        [StringLength(450)]
        public string? VerifiedBy { get; set; }

        public DateTimeOffset? VerifiedAt { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        public bool IsVirusScanPassed { get; set; } = false;

        public DateTimeOffset? VirusScanDate { get; set; }

        [StringLength(20)]
        public string AccessLevel { get; set; } = "Private";

        [StringLength(100)]
        public string? EncryptionKey { get; set; }

        // Audit timestamps
        public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Legacy property for backward compatibility
        public string? AdminFeedback { get; set; }

        // Navigation property
        public virtual Application Application { get; set; } = null!;
    }
}