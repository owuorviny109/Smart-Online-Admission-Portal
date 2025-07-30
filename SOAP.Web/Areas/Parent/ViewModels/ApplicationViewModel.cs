using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Areas.Parent.ViewModels
{
    public class ApplicationViewModel
    {
        [Required]
        [StringLength(20)]
        [Display(Name = "KCPE Index Number")]
        public string KcpeIndexNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Student Name")]
        public string StudentName { get; set; } = string.Empty;

        [Required]
        [Range(10, 25)]
        [Display(Name = "Student Age")]
        public int StudentAge { get; set; }

        [Required]
        [StringLength(15)]
        [Display(Name = "Parent Phone")]
        public string ParentPhone { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Parent Name")]
        public string ParentName { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        [Display(Name = "Emergency Contact")]
        public string EmergencyContact { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Emergency Contact Name")]
        public string EmergencyName { get; set; } = string.Empty;

        [Display(Name = "Home Address")]
        public string? HomeAddress { get; set; }

        [Display(Name = "Boarding Status")]
        public string BoardingStatus { get; set; } = "Day";

        [Display(Name = "Medical Conditions")]
        public string? MedicalConditions { get; set; }

        public int SchoolId { get; set; }
    }
}