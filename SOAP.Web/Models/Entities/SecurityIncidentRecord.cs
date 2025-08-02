using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    /// <summary>
    /// Security incident tracking for monitoring and response
    /// Helps identify patterns and potential security threats
    /// </summary>
    public class SecurityIncidentRecord
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string IncidentType { get; set; } = string.Empty; // BRUTE_FORCE, SUSPICIOUS_ACCESS, DATA_BREACH, etc.

        public SecurityIncidentSeverity Severity { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? AffectedUserId { get; set; }

        [MaxLength(45)]
        public string? SourceIpAddress { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Open"; // Open, Investigating, Resolved, Closed

        [MaxLength(500)]
        public string? AutomaticResponse { get; set; } // Actions taken automatically

        public string? ManualResponse { get; set; } // Actions taken manually

        public DateTimeOffset DetectedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? ResolvedAt { get; set; }

        [MaxLength(450)]
        public string? ResolvedBy { get; set; } // Admin user who resolved

        // Note: AffectedUserId and ResolvedBy are stored as strings for flexibility
        // No direct navigation properties due to type mismatch with User.Id (int)
    }

    public enum SecurityIncidentSeverity
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}