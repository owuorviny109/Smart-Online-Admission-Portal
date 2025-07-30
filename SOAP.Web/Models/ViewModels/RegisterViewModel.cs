using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Phone Number")]
        [StringLength(15)]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; }

        [Display(Name = "School")]
        public int? SchoolId { get; set; }
    }
}