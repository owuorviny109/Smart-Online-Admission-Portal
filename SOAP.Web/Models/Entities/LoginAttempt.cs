using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    /// <summary>
    /// Login attempt tracking for security monitoring and brute force protection
    /// Helps identify suspicious login patterns and implement rate limiting
    /// </summary>
    public class LoginAttempt
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        public bool Success { get; set; }

        [MaxLength(200)]
        public string? FailureReason { get; set; } // INVALID_PHONE, INVALID_OTP, ACCOUNT_LOCKED, etc.

        public DateTimeOffset AttemptedAt { get; set; } = DateTimeOffset.UtcNow;

        public string? OtpCode { get; set; } // Store hashed OTP for verification

        public DateTimeOffset? OtpExpiresAt { get; set; }

        public bool OtpUsed { get; set; } = false;

        // Navigation properties
        public User? User { get; set; }
    }
}