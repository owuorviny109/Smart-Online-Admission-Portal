using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    public class SecurityIncidentRecord
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string IncidentType { get; set; } = "";

        [Required]
        public int Severity { get; set; } // 1=Low, 2=Medium, 3=High, 4=Critical

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = "";

        [StringLength(450)]
        public string? AffectedUserId { get; set; }

        [StringLength(45)]
        public string? SourceIpAddress { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Open";

        [StringLength(500)]
        public string? AutomaticResponse { get; set; }

        public DateTimeOffset DetectedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? ResolvedAt { get; set; }

        [StringLength(450)]
        public string? AssignedTo { get; set; }
    }
}