using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    public class DataProcessingConsent
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string ConsentType { get; set; } = "";

        [Required]
        [StringLength(200)]
        public string Purpose { get; set; } = "";

        public bool ConsentGiven { get; set; }

        [Required]
        [StringLength(10)]
        public string ConsentVersion { get; set; } = "";

        public DateTimeOffset ConsentDate { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? ExpiryDate { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }
    }
}