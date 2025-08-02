using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;
using SOAP.Web.Models.Entities;
using SOAP.Web.Services.Interfaces;
using SOAP.Web.Utilities.Constants;
using System.Security.Cryptography;
using System.Text;

namespace SOAP.Web.Services
{
    /// <summary>
    /// Multi-Factor Authentication service with support for multiple contact methods
    /// </summary>
    public class MultiFactorAuthService : IMultiFactorAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<MultiFactorAuthService> _logger;
        private readonly Dictionary<string, OtpSession> _otpSessions = new();

        public MultiFactorAuthService(
            ApplicationDbContext context,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<MultiFactorAuthService> logger)
        {
            _context = context;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        /// <summary>
        /// Initiates MFA for Platform Admin with multiple contact options
        /// </summary>
        public async Task<MfaInitiationResult> InitiatePlatformAdminMfaAsync(string phoneNumber, string email, string ipAddress)
        {
            // Validate credentials against authorized contacts
            var (isValid, phoneUsed, emailUsed) = UserRoles.ValidatePlatformAdminCredentials(phoneNumber, email);
            
            if (!isValid)
            {
                _logger.LogCritical("SECURITY VIOLATION: Unauthorized Platform Admin MFA attempt from {Phone}/{Email} at IP {IP}", 
                    phoneNumber, email, ipAddress);
                
                await LogSecurityEventAsync("UNAUTHORIZED_PLATFORM_ADMIN_ATTEMPT", 
                    $"Phone: {phoneNumber}, Email: {email}, IP: {ipAddress}");
                
                return new MfaInitiationResult { Success = false, Error = "Unauthorized access attempt" };
            }

            // Generate secure OTP
            var otpCode = GenerateSecureOtp();
            var sessionId = Guid.NewGuid().ToString();
            
            // Store OTP session with expiration
            var otpSession = new OtpSession
            {
                SessionId = sessionId,
                PhoneNumber = phoneUsed,
                Email = emailUsed,
                OtpCode = otpCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5), // 5-minute expiration
                AttemptsRemaining = 3,
                IpAddress = ipAddress
            };

            _otpSessions[sessionId] = otpSession;

            // Send OTP via multiple channels for redundancy
            var smsTask = SendOtpViaSmsAsync(phoneUsed, otpCode);
            var emailTask = SendOtpViaEmailAsync(emailUsed, otpCode, ipAddress);

            await Task.WhenAll(smsTask, emailTask);

            _logger.LogInformation("Platform Admin MFA initiated for {Phone}/{Email} from IP {IP}", 
                phoneUsed, emailUsed, ipAddress);

            return new MfaInitiationResult 
            { 
                Success = true, 
                SessionId = sessionId,
                PhoneUsed = MaskPhoneNumber(phoneUsed),
                EmailUsed = MaskEmail(emailUsed),
                ExpiresIn = TimeSpan.FromMinutes(5)
            };
        }

        /// <summary>
        /// Verifies OTP with advanced security checks
        /// </summary>
        public async Task<MfaVerificationResult> VerifyOtpAsync(string sessionId, string otpCode, string ipAddress)
        {
            if (!_otpSessions.TryGetValue(sessionId, out var session))
            {
                _logger.LogWarning("Invalid OTP session attempted: {SessionId} from IP {IP}", sessionId, ipAddress);
                return new MfaVerificationResult { Success = false, Error = "Invalid session" };
            }

            // Check expiration
            if (DateTime.UtcNow > session.ExpiresAt)
            {
                _otpSessions.Remove(sessionId);
                _logger.LogWarning("Expired OTP session attempted: {SessionId}", sessionId);
                return new MfaVerificationResult { Success = false, Error = "OTP expired" };
            }

            // Check IP consistency (optional security measure)
            if (session.IpAddress != ipAddress)
            {
                _logger.LogWarning("OTP verification from different IP. Original: {OriginalIP}, Current: {CurrentIP}", 
                    session.IpAddress, ipAddress);
                // Don't block, but log for monitoring
            }

            // Check attempts remaining
            if (session.AttemptsRemaining <= 0)
            {
                _otpSessions.Remove(sessionId);
                _logger.LogWarning("OTP session exhausted attempts: {SessionId}", sessionId);
                return new MfaVerificationResult { Success = false, Error = "Too many failed attempts" };
            }

            // Verify OTP
            if (session.OtpCode != otpCode)
            {
                session.AttemptsRemaining--;
                _logger.LogWarning("Invalid OTP attempt for session {SessionId}. Attempts remaining: {Remaining}", 
                    sessionId, session.AttemptsRemaining);
                
                return new MfaVerificationResult 
                { 
                    Success = false, 
                    Error = "Invalid OTP", 
                    AttemptsRemaining = session.AttemptsRemaining 
                };
            }

            // Success! Clean up session
            _otpSessions.Remove(sessionId);
            
            // Generate secure session token
            var sessionToken = await GenerateSecureSessionTokenAsync(session.PhoneNumber, session.Email);
            
            _logger.LogInformation("Platform Admin MFA successful for {Phone}/{Email}", 
                session.PhoneNumber, session.Email);

            await LogSecurityEventAsync("PLATFORM_ADMIN_LOGIN_SUCCESS", 
                $"Phone: {session.PhoneNumber}, Email: {session.Email}, IP: {ipAddress}");

            return new MfaVerificationResult 
            { 
                Success = true, 
                SessionToken = sessionToken,
                PhoneUsed = session.PhoneNumber,
                EmailUsed = session.Email
            };
        }

        /// <summary>
        /// Provides backup access options if primary method fails
        /// </summary>
        public async Task<BackupAccessResult> InitiateBackupAccessAsync(string primaryPhone, string backupContactMethod)
        {
            var (phones, emails) = UserRoles.GetPlatformAdminContacts();
            
            // Verify the backup method is authorized
            var isAuthorizedPhone = phones.Contains(backupContactMethod);
            var isAuthorizedEmail = emails.Contains(backupContactMethod);
            
            if (!isAuthorizedPhone && !isAuthorizedEmail)
            {
                _logger.LogCritical("SECURITY VIOLATION: Unauthorized backup access attempt with {BackupMethod}", backupContactMethod);
                return new BackupAccessResult { Success = false, Error = "Unauthorized backup method" };
            }

            // Generate backup access code (longer expiration for backup)
            var backupCode = GenerateSecureOtp(8); // 8-digit code for backup
            var sessionId = Guid.NewGuid().ToString();
            
            var backupSession = new OtpSession
            {
                SessionId = sessionId,
                PhoneNumber = isAuthorizedPhone ? backupContactMethod : primaryPhone,
                Email = isAuthorizedEmail ? backupContactMethod : emails[0],
                OtpCode = backupCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15), // Longer expiration for backup
                AttemptsRemaining = 5, // More attempts for backup
                IsBackupAccess = true
            };

            _otpSessions[sessionId] = backupSession;

            // Send backup code
            if (isAuthorizedPhone)
            {
                await SendBackupCodeViaSmsAsync(backupContactMethod, backupCode);
            }
            else
            {
                await SendBackupCodeViaEmailAsync(backupContactMethod, backupCode);
            }

            _logger.LogInformation("Backup access initiated for Platform Admin using {Method}", 
                MaskContactMethod(backupContactMethod));

            return new BackupAccessResult 
            { 
                Success = true, 
                SessionId = sessionId,
                ContactMethod = MaskContactMethod(backupContactMethod),
                ExpiresIn = TimeSpan.FromMinutes(15)
            };
        }

        private string GenerateSecureOtp(int length = 6)
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var random = new Random(BitConverter.ToInt32(bytes, 0));
            
            var otp = "";
            for (int i = 0; i < length; i++)
            {
                otp += random.Next(0, 10).ToString();
            }
            
            return otp;
        }

        private async Task<string> GenerateSecureSessionTokenAsync(string phone, string email)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes($"{phone}:{email}:{DateTime.UtcNow:yyyyMMddHH}"));
            var tokenData = $"{phone}:{email}:{DateTime.UtcNow.Ticks}:{Guid.NewGuid()}";
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(tokenData));
            return Convert.ToBase64String(hash);
        }

        private async Task SendOtpViaSmsAsync(string phoneNumber, string otpCode)
        {
            var message = $"SOAP Platform Admin Login\nOTP: {otpCode}\nValid for 5 minutes.\nIf you didn't request this, contact support immediately.";
            await _smsService.SendSmsAsync(phoneNumber, message);
        }

        private async Task SendOtpViaEmailAsync(string email, string otpCode, string ipAddress)
        {
            var subject = "SOAP Platform Admin Login - OTP Verification";
            var body = $@"
                <h2>Platform Admin Login Verification</h2>
                <p>Your OTP code is: <strong>{otpCode}</strong></p>
                <p>This code is valid for 5 minutes.</p>
                <p>Login attempt from IP: {ipAddress}</p>
                <p>If you didn't request this login, please contact support immediately.</p>
            ";
            
            await _emailService.SendEmailAsync(email, subject, body);
        }

        private async Task SendBackupCodeViaSmsAsync(string phoneNumber, string backupCode)
        {
            var message = $"SOAP Platform Admin Backup Access\nBackup Code: {backupCode}\nValid for 15 minutes.\nThis is a backup access method.";
            await _smsService.SendSmsAsync(phoneNumber, message);
        }

        private async Task SendBackupCodeViaEmailAsync(string email, string backupCode)
        {
            var subject = "SOAP Platform Admin Backup Access";
            var body = $@"
                <h2>Platform Admin Backup Access</h2>
                <p>Your backup access code is: <strong>{backupCode}</strong></p>
                <p>This code is valid for 15 minutes.</p>
                <p>This is a backup access method for your Platform Admin account.</p>
            ";
            
            await _emailService.SendEmailAsync(email, subject, body);
        }

        private string MaskPhoneNumber(string phone)
        {
            if (phone.Length <= 4) return phone;
            return phone.Substring(0, 4) + "****" + phone.Substring(phone.Length - 2);
        }

        private string MaskEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2) return email;
            
            var username = parts[0];
            var domain = parts[1];
            
            if (username.Length <= 2) return email;
            return username.Substring(0, 2) + "****@" + domain;
        }

        private string MaskContactMethod(string contact)
        {
            return contact.Contains("@") ? MaskEmail(contact) : MaskPhoneNumber(contact);
        }

        private async Task LogSecurityEventAsync(string eventType, string details)
        {
            var securityLog = new SecurityAuditLog
            {
                EventType = eventType,
                Details = details,
                Timestamp = DateTimeOffset.UtcNow,
                Success = eventType.Contains("SUCCESS")
            };

            _context.SecurityAuditLogs.Add(securityLog);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cleanup expired OTP sessions
        /// </summary>
        public void CleanupExpiredSessions()
        {
            var expiredSessions = _otpSessions
                .Where(kvp => DateTime.UtcNow > kvp.Value.ExpiresAt)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var sessionId in expiredSessions)
            {
                _otpSessions.Remove(sessionId);
            }
        }
    }

    // Supporting classes
    public class OtpSession
    {
        public string SessionId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int AttemptsRemaining { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public bool IsBackupAccess { get; set; }
    }

    public class MfaInitiationResult
    {
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string PhoneUsed { get; set; } = string.Empty;
        public string EmailUsed { get; set; } = string.Empty;
        public TimeSpan ExpiresIn { get; set; }
    }

    public class MfaVerificationResult
    {
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public string SessionToken { get; set; } = string.Empty;
        public string PhoneUsed { get; set; } = string.Empty;
        public string EmailUsed { get; set; } = string.Empty;
        public int AttemptsRemaining { get; set; }
    }

    public class BackupAccessResult
    {
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string ContactMethod { get; set; } = string.Empty;
        public TimeSpan ExpiresIn { get; set; }
    }
}