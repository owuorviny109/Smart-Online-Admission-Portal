using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    /// <summary>
    /// Security audit log for tracking all security-related events
    /// Complies with Kenya Data Protection Act requirements for audit trails
    /// </summary>
    public class SecurityAuditLog
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string EventType { get; set; } = string.Empty; // LOGIN_SUCCESS, LOGIN_FAILURE, DATA_ACCESS, etc.

        [MaxLength(450)]
        public string? UserId { get; set; } // User ID if authenticated

        [MaxLength(50)]
        public string? UserRole { get; set; } // Parent, Admin

        [MaxLength(45)] // IPv6 support
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [MaxLength(200)]
        public string? ResourceAccessed { get; set; } // Controller/Action or Entity name

        [MaxLength(100)]
        public string? ActionPerformed { get; set; } // CREATE, READ, UPDATE, DELETE

        public bool Success { get; set; }

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        public string? Details { get; set; } // Detailed description of the event

        public string? AdditionalData { get; set; } // JSON data for additional context

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        // Note: UserId is stored as string for flexibility
        // No direct navigation property due to type mismatch with User.Id (int)
    }
}