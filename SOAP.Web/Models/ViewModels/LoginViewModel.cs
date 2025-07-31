using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.ViewModels
{
    /// <summary>
    /// Login view model with validation
    /// Demonstrates: Encapsulation, Data Validation
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain only digits")]
        [Display(Name = "OTP Code")]
        public string Otp { get; set; } = "";

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; } = false;

        public string? ReturnUrl { get; set; }
    }
}