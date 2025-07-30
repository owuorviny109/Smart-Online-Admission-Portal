using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    /// <summary>
    /// Security audit log for tracking all security-relevant events
    /// Implements comprehensive audit trail as per security requirements
    /// </summary>
    public class SecurityAuditLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string EventType { get; set; } // LOGIN_SUCCESS, LOGIN_FAILED, DATA_ACCESS, etc.

        [StringLength(450)]
        public string? UserId { get; set; }

        [StringLength(50)]
        public string? UserRole { get; set; }

        [StringLength(45)] // IPv6 support
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(200)]
        public string? ResourceAccessed { get; set; }

        [StringLength(100)]
        public string? ActionPerformed { get; set; }

        public bool Success { get; set; }

        [StringLength(500)]
        public string? FailureReason { get; set; }

        public string? AdditionalData { get; set; } // JSON data for additional context

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}