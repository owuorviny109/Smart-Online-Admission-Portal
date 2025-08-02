using SOAP.Web.Models.Entities;

namespace SOAP.Web.Services.Interfaces
{
    /// <summary>
    /// Advanced security service with MFA, OTP, and anti-hacking measures
    /// </summary>
    public interface IAdvancedSecurityService
    {
        /// <summary>
        /// Generates and sends OTP for Platform Admin login
        /// </summary>
        Task<bool> SendPlatformAdminOtpAsync(string phoneNumber, string email);

        /// <summary>
        /// Verifies OTP for Platform Admin login
        /// </summary>
        Task<bool> VerifyPlatformAdminOtpAsync(string phoneNumber, string otpCode);

        /// <summary>
        /// Generates secure session token after successful MFA
        /// </summary>
        Task<string> GenerateSecureSessionTokenAsync(User user);

        /// <summary>
        /// Validates secure session token
        /// </summary>
        Task<bool> ValidateSecureSessionTokenAsync(string token, int userId);

        /// <summary>
        /// Detects and blocks brute force attacks
        /// </summary>
        Task<bool> IsUnderBruteForceAttackAsync(string phoneNumber, string ipAddress);

        /// <summary>
        /// Logs security events and sends alerts
        /// </summary>
        Task LogSecurityEventAsync(string eventType, string details, User user = null, string ipAddress = null);

        /// <summary>
        /// Checks if IP address is suspicious or blacklisted
        /// </summary>
        Task<bool> IsSuspiciousIpAddressAsync(string ipAddress);

        /// <summary>
        /// Implements rate limiting for sensitive operations
        /// </summary>
        Task<bool> IsRateLimitExceededAsync(string identifier, string operation);

        /// <summary>
        /// Validates device fingerprint for additional security
        /// </summary>
        Task<bool> ValidateDeviceFingerprintAsync(string deviceFingerprint, User user);

        /// <summary>
        /// Sends immediate security alert to Platform Admin
        /// </summary>
        Task SendSecurityAlertAsync(string alertType, string message, string details = null);

        /// <summary>
        /// Implements account lockout after failed attempts
        /// </summary>
        Task<bool> ShouldLockAccountAsync(string phoneNumber);

        /// <summary>
        /// Validates login attempt with advanced security checks
        /// </summary>
        Task<SecurityValidationResult> ValidateLoginAttemptAsync(string phoneNumber, string ipAddress, string userAgent, string deviceFingerprint);
    }

    /// <summary>
    /// Result of security validation
    /// </summary>
    public class SecurityValidationResult
    {
        public bool IsAllowed { get; set; }
        public string ReasonIfBlocked { get; set; } = string.Empty;
        public bool RequiresMfa { get; set; }
        public bool RequiresDeviceVerification { get; set; }
        public int RemainingAttempts { get; set; }
        public TimeSpan? LockoutDuration { get; set; }
    }
}