using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models
{
    public class SchoolStudent
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string KcpeIndexNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string StudentName { get; set; }

        public int? KcpeScore { get; set; }

        [Required]
        public int SchoolId { get; set; }

        [Required]
        public int Year { get; set; }

        public bool HasApplied { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual School School { get; set; }
    }
}