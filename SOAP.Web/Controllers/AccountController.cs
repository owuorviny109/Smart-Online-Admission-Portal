using Microsoft.AspNetCore.Mvc;
using SOAP.Web.Data;
using SOAP.Web.Services.Interfaces;
using SOAP.Web.Models;
using SOAP.Web.Models.ViewModels;

namespace SOAP.Web.Controllers
{
    /// <summary>
    /// Account controller for authentication operations
    /// Demonstrates: Inheritance (from BaseController), SRP, DIP
    /// </summary>
    public class AccountController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public AccountController(
            ApplicationDbContext context,
            INotificationService notificationService,
            ILogger<AccountController> logger,
            ISecurityAuditService auditService) : base(logger, auditService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        /// <summary>
        /// Displays login page
        /// </summary>
        public async Task<IActionResult> Login()
        {
            await LogSecurityEventAsync("LOGIN_PAGE_ACCESSED", true);
            return View();
        }

        /// <summary>
        /// Processes login attempt
        /// Demonstrates: Template Method Pattern (using base class validation)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Use base class validation method
            var validationResult = HandleModelValidation(model);
            if (validationResult != null)
                return validationResult;

            try
            {
                // TODO: Implement OTP verification logic
                await LogSecurityEventAsync("LOGIN_ATTEMPT", true, $"Phone: {MaskPhoneNumber(model.PhoneNumber)}");
                
                // For now, redirect to home
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for phone number: {Phone}", MaskPhoneNumber(model.PhoneNumber));
                await LogSecurityEventAsync("LOGIN_FAILED", false, ex.Message);
                
                ModelState.AddModelError("", "Login failed. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// Displays registration page
        /// </summary>
        public async Task<IActionResult> Register()
        {
            await LogSecurityEventAsync("REGISTER_PAGE_ACCESSED", true);
            return View();
        }

        /// <summary>
        /// Processes registration
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Use base class validation method
            var validationResult = HandleModelValidation(model);
            if (validationResult != null)
                return validationResult;

            try
            {
                // TODO: Implement registration logic
                await LogSecurityEventAsync("REGISTRATION_ATTEMPT", true, $"Phone: {MaskPhoneNumber(model.PhoneNumber)}");
                
                // Send welcome SMS
                await _notificationService.SendNotificationAsync(
                    NotificationType.Sms,
                    model.PhoneNumber,
                    "Welcome to SOAP! Your registration was successful.",
                    new NotificationContext { Subject = "Registration Successful" });

                TempData["SuccessMessage"] = "Registration successful! Please check your phone for confirmation.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for phone number: {Phone}", MaskPhoneNumber(model.PhoneNumber));
                await LogSecurityEventAsync("REGISTRATION_FAILED", false, ex.Message);
                
                ModelState.AddModelError("", "Registration failed. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// Processes logout
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            try
            {
                await LogSecurityEventAsync("LOGOUT", true, $"User: {GetCurrentUserId()}");
                
                // TODO: Implement logout logic (clear session, etc.)
                
                TempData["InfoMessage"] = "You have been logged out successfully.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed for user: {UserId}", GetCurrentUserId());
                await LogSecurityEventAsync("LOGOUT_FAILED", false, ex.Message);
                
                return CreateErrorResponse("Logout failed. Please try again.");
            }
        }

        /// <summary>
        /// Masks phone number for logging (privacy protection)
        /// Encapsulation: Private method for data protection
        /// </summary>
        private string MaskPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
                return "****";

            return phoneNumber.Substring(0, 3) + "****" + phoneNumber.Substring(phoneNumber.Length - 2);
        }
    }
}