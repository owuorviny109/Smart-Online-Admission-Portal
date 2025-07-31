using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    public class SecurityAuditLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string EventType { get; set; } = "";

        [StringLength(100)]
        public string UserId { get; set; } = "";

        [StringLength(20)]
        public string UserRole { get; set; } = "";

        [StringLength(45)]
        public string IpAddress { get; set; } = "";

        [StringLength(500)]
        public string UserAgent { get; set; } = "";

        [StringLength(200)]
        public string ResourceAccessed { get; set; } = "";

        [StringLength(200)]
        public string ActionPerformed { get; set; } = "";

        public bool Success { get; set; }

        [StringLength(500)]
        public string FailureReason { get; set; } = "";

        public string AdditionalData { get; set; } = "";

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}