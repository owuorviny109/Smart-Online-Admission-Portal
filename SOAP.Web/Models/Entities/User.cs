using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = "";

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = ""; // Parent, SchoolAdmin, SuperAdmin

        public int? SchoolId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Security properties
        public DateTimeOffset? LastLoginAt { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;

        public DateTimeOffset? LockedUntil { get; set; }

        // GDPR compliance properties
        public DateTimeOffset? DeletionDate { get; set; }

        [StringLength(200)]
        public string? DeletionReason { get; set; }

        // Navigation properties
        public virtual School? School { get; set; }
    }
}