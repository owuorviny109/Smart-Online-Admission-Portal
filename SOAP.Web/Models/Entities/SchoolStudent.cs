using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    public class SchoolStudent
    {
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }

        [Required]
        [StringLength(20)]
        public string KcpeIndexNumber { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string StudentName { get; set; } = "";

        public int KcpeScore { get; set; }

        [Required]
        public int Year { get; set; }

        [StringLength(20)]
        public string PlacementStatus { get; set; } = "Placed";

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        public School School { get; set; } = null!;
    }
}