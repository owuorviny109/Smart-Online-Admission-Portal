using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    public class LoginAttempt
    {
        public int Id { get; set; }

        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = "";

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public bool Success { get; set; }

        [StringLength(200)]
        public string? FailureReason { get; set; }

        public DateTimeOffset AttemptedAt { get; set; } = DateTimeOffset.UtcNow;

        public int OtpAttempts { get; set; } = 0;

        public bool IsBlocked { get; set; } = false;
    }
}