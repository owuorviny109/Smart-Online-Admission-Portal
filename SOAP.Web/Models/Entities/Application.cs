using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    public class Application
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string KcpeIndexNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string StudentName { get; set; }

        [Required]
        public int StudentAge { get; set; }

        [Required]
        [StringLength(15)]
        public string ParentPhone { get; set; }

        [Required]
        [StringLength(100)]
        public string ParentName { get; set; }

        [Required]
        [StringLength(15)]
        public string EmergencyContact { get; set; }

        [Required]
        [StringLength(100)]
        public string EmergencyName { get; set; }

        public string? HomeAddress { get; set; }

        [StringLength(20)]
        public string BoardingStatus { get; set; } = "Day";

        public string? MedicalConditions { get; set; }

        [Required]
        public int SchoolId { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [StringLength(10)]
        public string? AdmissionCode { get; set; }

        public bool CheckedIn { get; set; } = false;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Security and audit properties
        public DateTimeOffset? SubmittedAt { get; set; }

        public DateTimeOffset? ReviewedAt { get; set; }

        [StringLength(450)]
        public string? ReviewedBy { get; set; }

        // Navigation properties
        public virtual School School { get; set; } = null!;
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}