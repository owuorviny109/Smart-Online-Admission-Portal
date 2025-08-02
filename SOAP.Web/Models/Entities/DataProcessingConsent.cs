using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    /// <summary>
    /// Data processing consent tracking for GDPR/Kenya Data Protection Act compliance
    /// Records user consent for different types of data processing
    /// </summary>
    public class DataProcessingConsent
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ConsentType { get; set; } = string.Empty; // PERSONAL_DATA, DOCUMENT_STORAGE, SMS_NOTIFICATIONS, etc.

        [Required]
        [MaxLength(200)]
        public string Purpose { get; set; } = string.Empty; // Purpose of data processing

        public bool ConsentGiven { get; set; }

        [Required]
        [MaxLength(10)]
        public string ConsentVersion { get; set; } = string.Empty; // Version of consent terms

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        public DateTimeOffset ConsentDate { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? WithdrawnDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Note: UserId is stored as string for flexibility
        // No direct navigation property due to type mismatch with User.Id (int)
    }
}