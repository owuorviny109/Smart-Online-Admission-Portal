using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Phone Number")]
        [StringLength(15)]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "OTP Code")]
        [StringLength(6)]
        public string OtpCode { get; set; }
    }
}