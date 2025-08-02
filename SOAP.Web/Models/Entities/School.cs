using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    public class School
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; }

        [Required]
        [StringLength(50)]
        public string County { get; set; }

        [StringLength(15)]
        public string? ContactPhone { get; set; }

        [StringLength(100)]
        public string? ContactEmail { get; set; }

        [StringLength(500)]
        public string? LogoPath { get; set; }

        [StringLength(20)]
        public string? Subdomain { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
        public virtual ICollection<SchoolStudent> SchoolStudents { get; set; } = new List<SchoolStudent>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}