using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    public class SmsLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string MessageType { get; set; } = "";

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = "";

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public int? ApplicationId { get; set; }

        public DateTimeOffset? SentAt { get; set; }
        public DateTimeOffset? DeliveredAt { get; set; }
        
        [StringLength(200)]
        public string? FailureReason { get; set; }
        
        [StringLength(100)]
        public string? ProviderId { get; set; }
        
        public decimal? Cost { get; set; }
        
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;
        public DateTimeOffset? NextRetryAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation property
        public virtual Application? Application { get; set; }
    }
}