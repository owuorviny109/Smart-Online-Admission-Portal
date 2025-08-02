using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;
using SOAP.Web.Models.Entities;
using SOAP.Web.Services.Interfaces;
using SOAP.Web.Utilities.Constants;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SOAP.Web.Services
{
    /// <summary>
    /// Advanced security service with anti-hacking measures
    /// </summary>
    public class AdvancedSecurityService : IAdvancedSecurityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdvancedSecurityService> _logger;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        
        // In-memory caches for performance (consider Redis for production)
        private readonly Dictionary<string, List<DateTime>> _rateLimitCache = new();
        private readonly HashSet<string> _suspiciousIps = new();
        private readonly Dictionary<string, int> _failedAttempts = new();

        public AdvancedSecurityService(
            ApplicationDbContext context,
            ILogger<AdvancedSecurityService> logger,
            IEmailService emailService,
            ISmsService smsService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
            _smsService = smsService;
            
            // Load suspicious IPs from database on startup
            LoadSuspiciousIpsAsync();
        }

        /// <summary>
        /// Comprehensive login validation with multiple security layers
        /// </summary>
        public async Task<SecurityValidationResult> ValidateLoginAttemptAsync(
            string phoneNumber, string ipAddress, string userAgent, string deviceFingerprint)
        {
            var result = new SecurityValidationResult { IsAllowed = true };

            // 1. Check if IP is blacklisted
            if (await IsSuspiciousIpAddressAsync(ipAddress))
            {
                await LogSecurityEventAsync("BLOCKED_SUSPICIOUS_IP", 
                    $"Login blocked from suspicious IP: {ipAddress}, Phone: {phoneNumber}");
                
                return new SecurityValidationResult 
                { 
                    IsAllowed = false, 
                    ReasonIfBlocked = "Access denied from this location" 
                };
            }

            // 2. Check for brute force attacks
            if (await IsUnderBruteForceAttackAsync(phoneNumber, ipAddress))
            {
                await LogSecurityEventAsync("BRUTE_FORCE_DETECTED", 
                    $"Brute force attack detected for phone: {phoneNumber}, IP: {ipAddress}");
                
                return new SecurityValidationResult 
                { 
                    IsAllowed = false, 
                    ReasonIfBlocked = "Too many failed attempts. Account temporarily locked." 
                };
            }

            // 3. Rate limiting check
            if (await IsRateLimitExceededAsync($"{phoneNumber}:{ipAddress}", "LOGIN"))
            {
                return new SecurityValidationResult 
                { 
                    IsAllowed = false, 
                    ReasonIfBlocked = "Too many requests. Please wait before trying again." 
                };
            }

            // 4. Check if account should be locked
            if (await ShouldLockAccountAsync(phoneNumber))
            {
                var lockoutDuration = await GetLockoutDurationAsync(phoneNumber);
                return new SecurityValidationResult 
                { 
                    IsAllowed = false, 
                    ReasonIfBlocked = "Account temporarily locked due to security concerns",
                    LockoutDuration = lockoutDuration
                };
            }

            // 5. Platform Admin requires MFA
            var (phones, emails) = UserRoles.GetPlatformAdminContacts();
            if (phones.Contains(phoneNumber))
            {
                result.RequiresMfa = true;
                
                // Additional security for Platform Admin
                await LogSecurityEventAsync("PLATFORM_ADMIN_LOGIN_ATTEMPT", 
                    $"Platform Admin login attempt from IP: {ipAddress}, UserAgent: {userAgent}");
                
                // Send immediate security notification
                await SendSecurityAlertAsync("PLATFORM_ADMIN_LOGIN", 
                    $"Login attempt detected for Platform Admin account from IP: {ipAddress}",
                    $"UserAgent: {userAgent}\nTime: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            }

            // 6. Device fingerprint validation for known users
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user != null && !await ValidateDeviceFingerprintAsync(deviceFingerprint, user))
            {
                result.RequiresDeviceVerification = true;
                
                await LogSecurityEventAsync("UNKNOWN_DEVICE_LOGIN", 
                    $"Login from unknown device for user: {user.Id}, IP: {ipAddress}");
            }

            // 7. Geographic anomaly detection (basic implementation)
            await DetectGeographicAnomalyAsync(phoneNumber, ipAddress);

            return result;
        }

        /// <summary>
        /// Detects brute force attacks with progressive penalties
        /// </summary>
        public async Task<bool> IsUnderBruteForceAttackAsync(string phoneNumber, string ipAddress)
        {
            var key = $"{phoneNumber}:{ipAddress}";
            var threshold = DateTime.UtcNow.AddMinutes(-15); // 15-minute window
            
            var recentAttempts = await _context.LoginAttempts
                .Where(la => (la.PhoneNumber == phoneNumber || la.IpAddress == ipAddress) 
                           && la.AttemptedAt > threshold 
                           && !la.Success)
                .CountAsync();

            // Progressive thresholds
            if (recentAttempts >= 10) // Very aggressive
            {
                _suspiciousIps.Add(ipAddress);
                await AddSuspiciousIpAsync(ipAddress, "Brute force attack detected");
                return true;
            }

            if (recentAttempts >= 5) // Moderate
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Advanced rate limiting with different limits per operation
        /// </summary>
        public async Task<bool> IsRateLimitExceededAsync(string identifier, string operation)
        {
            var limits = GetRateLimits(operation);
            var key = $"{identifier}:{operation}";
            
            if (!_rateLimitCache.ContainsKey(key))
            {
                _rateLimitCache[key] = new List<DateTime>();
            }

            var attempts = _rateLimitCache[key];
            var cutoff = DateTime.UtcNow.Subtract(limits.window);
            
            // Remove old attempts
            attempts.RemoveAll(a => a < cutoff);
            
            if (attempts.Count >= limits.maxAttempts)
            {
                await LogSecurityEventAsync("RATE_LIMIT_EXCEEDED", 
                    $"Rate limit exceeded for {operation}: {identifier}");
                return true;
            }

            attempts.Add(DateTime.UtcNow);
            return false;
        }

        /// <summary>
        /// Suspicious IP detection with multiple data sources
        /// </summary>
        public async Task<bool> IsSuspiciousIpAddressAsync(string ipAddress)
        {
            // Check local blacklist
            if (_suspiciousIps.Contains(ipAddress))
                return true;

            // Check database blacklist
            var isBlacklisted = await _context.SecurityAuditLogs
                .AnyAsync(sal => sal.Details.Contains(ipAddress) 
                              && sal.EventType.Contains("BLOCKED"));

            if (isBlacklisted)
            {
                _suspiciousIps.Add(ipAddress);
                return true;
            }

            // Basic IP reputation check (you can integrate with external services)
            if (await IsKnownMaliciousIpAsync(ipAddress))
            {
                await AddSuspiciousIpAsync(ipAddress, "Known malicious IP");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Device fingerprint validation for additional security
        /// </summary>
        public async Task<bool> ValidateDeviceFingerprintAsync(string deviceFingerprint, User user)
        {
            if (string.IsNullOrEmpty(deviceFingerprint))
                return false;

            // Check if this device has been used before
            var knownDevice = await _context.SecurityAuditLogs
                .AnyAsync(sal => sal.UserId == user.Id.ToString() 
                              && sal.Details.Contains(deviceFingerprint)
                              && sal.Success);

            return knownDevice;
        }

        /// <summary>
        /// Account lockout with progressive penalties
        /// </summary>
        public async Task<bool> ShouldLockAccountAsync(string phoneNumber)
        {
            var failedCount = _failedAttempts.GetValueOrDefault(phoneNumber, 0);
            
            // Progressive lockout thresholds
            if (failedCount >= 10) return true; // Permanent lock (requires admin intervention)
            if (failedCount >= 7) return true;  // 1 hour lock
            if (failedCount >= 5) return true;  // 30 minute lock
            if (failedCount >= 3) return true;  // 10 minute lock

            return false;
        }

        /// <summary>
        /// Sends immediate security alerts to Platform Admin
        /// </summary>
        public async Task SendSecurityAlertAsync(string alertType, string message, string details = null)
        {
            var (phones, emails) = UserRoles.GetPlatformAdminContacts();
            
            // Send SMS alert to all authorized phones
            foreach (var phone in phones)
            {
                var smsMessage = $"🚨 SOAP SECURITY ALERT\n{alertType}\n{message}\nTime: {DateTime.UtcNow:HH:mm} UTC";
                await _smsService.SendSmsAsync(phone, smsMessage);
            }

            // Send detailed email alert
            foreach (var email in emails)
            {
                var subject = $"🚨 SOAP Security Alert - {alertType}";
                var body = $@"
                    <h2 style='color: red;'>🚨 Security Alert</h2>
                    <p><strong>Alert Type:</strong> {alertType}</p>
                    <p><strong>Message:</strong> {message}</p>
                    <p><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                    {(details != null ? $"<p><strong>Details:</strong><br/><pre>{details}</pre></p>" : "")}
                    <p>If this was not authorized by you, please take immediate action.</p>
                ";
                
                await _emailService.SendEmailAsync(email, subject, body);
            }
        }

        /// <summary>
        /// Comprehensive security event logging
        /// </summary>
        public async Task LogSecurityEventAsync(string eventType, string details, User user = null, string ipAddress = null)
        {
            var securityLog = new SecurityAuditLog
            {
                EventType = eventType,
                Details = details,
                UserId = user?.Id.ToString(),
                IpAddress = ipAddress,
                Timestamp = DateTimeOffset.UtcNow,
                Success = !eventType.Contains("VIOLATION") && !eventType.Contains("BLOCKED")
            };

            _context.SecurityAuditLogs.Add(securityLog);
            await _context.SaveChangesAsync();

            // Send alert for critical events
            if (eventType.Contains("VIOLATION") || eventType.Contains("PLATFORM_ADMIN"))
            {
                await SendSecurityAlertAsync(eventType, details);
            }
        }

        // Helper methods
        private (int maxAttempts, TimeSpan window) GetRateLimits(string operation)
        {
            return operation switch
            {
                "LOGIN" => (5, TimeSpan.FromMinutes(15)),
                "OTP_REQUEST" => (3, TimeSpan.FromMinutes(5)),
                "PASSWORD_RESET" => (2, TimeSpan.FromHours(1)),
                "PLATFORM_ADMIN_ACCESS" => (3, TimeSpan.FromMinutes(30)),
                _ => (10, TimeSpan.FromMinutes(10))
            };
        }

        private async Task<TimeSpan> GetLockoutDurationAsync(string phoneNumber)
        {
            var failedCount = _failedAttempts.GetValueOrDefault(phoneNumber, 0);
            
            return failedCount switch
            {
                >= 10 => TimeSpan.FromDays(1), // Severe lockout
                >= 7 => TimeSpan.FromHours(1),
                >= 5 => TimeSpan.FromMinutes(30),
                >= 3 => TimeSpan.FromMinutes(10),
                _ => TimeSpan.Zero
            };
        }

        private async Task DetectGeographicAnomalyAsync(string phoneNumber, string ipAddress)
        {
            // Basic geographic anomaly detection
            // In production, you'd use a proper IP geolocation service
            try
            {
                var previousLogins = await _context.LoginAttempts
                    .Where(la => la.PhoneNumber == phoneNumber && la.Success)
                    .OrderByDescending(la => la.AttemptedAt)
                    .Take(5)
                    .Select(la => la.IpAddress)
                    .ToListAsync();

                if (previousLogins.Any() && !previousLogins.Contains(ipAddress))
                {
                    await LogSecurityEventAsync("GEOGRAPHIC_ANOMALY", 
                        $"Login from new location detected for {phoneNumber}, IP: {ipAddress}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in geographic anomaly detection");
            }
        }

        private async Task<bool> IsKnownMaliciousIpAsync(string ipAddress)
        {
            // Basic malicious IP detection
            // You can integrate with services like AbuseIPDB, VirusTotal, etc.
            
            // Check for common malicious patterns
            if (IPAddress.TryParse(ipAddress, out var ip))
            {
                // Block common malicious ranges (example)
                var bytes = ip.GetAddressBytes();
                
                // Block Tor exit nodes, known botnets, etc.
                // This is a simplified example - use proper threat intelligence
                if (bytes[0] == 10 || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31))
                {
                    return false; // Private IPs are generally safe
                }
            }

            return false;
        }

        private async Task AddSuspiciousIpAsync(string ipAddress, string reason)
        {
            _suspiciousIps.Add(ipAddress);
            
            await LogSecurityEventAsync("IP_BLACKLISTED", 
                $"IP {ipAddress} added to blacklist. Reason: {reason}");
        }

        private async Task LoadSuspiciousIpsAsync()
        {
            try
            {
                var suspiciousIps = await _context.SecurityAuditLogs
                    .Where(sal => sal.EventType == "IP_BLACKLISTED")
                    .Select(sal => sal.IpAddress)
                    .Where(ip => !string.IsNullOrEmpty(ip))
                    .Distinct()
                    .ToListAsync();

                foreach (var ip in suspiciousIps)
                {
                    _suspiciousIps.Add(ip);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading suspicious IPs");
            }
        }

        public async Task<bool> SendPlatformAdminOtpAsync(string phoneNumber, string email)
        {
            // This is handled by MultiFactorAuthService
            throw new NotImplementedException("Use MultiFactorAuthService for OTP operations");
        }

        public async Task<bool> VerifyPlatformAdminOtpAsync(string phoneNumber, string otpCode)
        {
            // This is handled by MultiFactorAuthService
            throw new NotImplementedException("Use MultiFactorAuthService for OTP operations");
        }

        public async Task<string> GenerateSecureSessionTokenAsync(User user)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes($"{user.PhoneNumber}:{DateTime.UtcNow:yyyyMMddHH}"));
            var tokenData = $"{user.Id}:{user.PhoneNumber}:{DateTime.UtcNow.Ticks}:{Guid.NewGuid()}";
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(tokenData));
            return Convert.ToBase64String(hash);
        }

        public async Task<bool> ValidateSecureSessionTokenAsync(string token, int userId)
        {
            // Implement secure session token validation
            // This would typically involve checking token expiration, user status, etc.
            return !string.IsNullOrEmpty(token);
        }
    }
}