using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;
using SOAP.Web.Models.Entities;

namespace SOAP.Web.Services.BackgroundServices
{
    public class SecurityMonitoringService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SecurityMonitoringService> _logger;

        public SecurityMonitoringService(IServiceProvider serviceProvider, ILogger<SecurityMonitoringService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Security Monitoring Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Check for suspicious activities every 5 minutes
                    await DetectBruteForceAttacksAsync(context);
                    await DetectUnusualAccessPatternsAsync(context);
                    await DetectDataExfiltrationAttemptsAsync(context);
                    await CleanupExpiredLockoutsAsync(context);

                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Security Monitoring Service");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Security Monitoring Service stopped");
        }

        private async Task DetectBruteForceAttacksAsync(ApplicationDbContext context)
        {
            var threshold = DateTimeOffset.UtcNow.AddMinutes(-15);
            
            // Detect IP-based brute force attacks
            var suspiciousIPs = await context.LoginAttempts
                .Where(log => !log.Success && log.AttemptedAt > threshold)
                .GroupBy(log => log.IpAddress)
                .Where(group => group.Count() >= 5)
                .Select(group => new { IpAddress = group.Key, Count = group.Count() })
                .ToListAsync();

            foreach (var suspiciousIP in suspiciousIPs)
            {
                if (string.IsNullOrEmpty(suspiciousIP.IpAddress)) continue;

                await CreateSecurityIncidentAsync(context, 
                    "BRUTE_FORCE_IP", 
                    SecurityIncidentSeverity.High,
                    $"Brute force attack detected from IP {suspiciousIP.IpAddress} with {suspiciousIP.Count} failed attempts in 15 minutes",
                    sourceIp: suspiciousIP.IpAddress);

                _logger.LogWarning("Brute force attack detected from IP: {IpAddress} with {Count} attempts", 
                    suspiciousIP.IpAddress, suspiciousIP.Count);
            }

            // Detect phone-based brute force attacks
            var suspiciousPhones = await context.LoginAttempts
                .Where(log => !log.Success && log.AttemptedAt > threshold)
                .GroupBy(log => log.PhoneNumber)
                .Where(group => group.Count() >= 3)
                .Select(group => new { PhoneNumber = group.Key, Count = group.Count() })
                .ToListAsync();

            foreach (var suspiciousPhone in suspiciousPhones)
            {
                await CreateSecurityIncidentAsync(context,
                    "BRUTE_FORCE_PHONE",
                    SecurityIncidentSeverity.Medium,
                    $"Multiple failed login attempts for phone {suspiciousPhone.PhoneNumber}: {suspiciousPhone.Count} attempts in 15 minutes");

                _logger.LogWarning("Multiple failed login attempts for phone: {PhoneNumber} with {Count} attempts",
                    suspiciousPhone.PhoneNumber, suspiciousPhone.Count);
            }
        }

        private async Task DetectUnusualAccessPatternsAsync(ApplicationDbContext context)
        {
            var threshold = DateTimeOffset.UtcNow.AddHours(-1);

            // Detect unusual access patterns (multiple IPs for same user)
            var unusualAccess = await context.SecurityAuditLogs
                .Where(log => log.Success && log.Timestamp > threshold && !string.IsNullOrEmpty(log.UserId))
                .GroupBy(log => log.UserId)
                .Where(group => group.Select(g => g.IpAddress).Distinct().Count() > 3)
                .Select(group => new { 
                    UserId = group.Key, 
                    IpCount = group.Select(g => g.IpAddress).Distinct().Count(),
                    IpAddresses = group.Select(g => g.IpAddress).Distinct().ToList()
                })
                .ToListAsync();

            foreach (var unusual in unusualAccess)
            {
                await CreateSecurityIncidentAsync(context,
                    "UNUSUAL_ACCESS_PATTERN",
                    SecurityIncidentSeverity.Medium,
                    $"User {unusual.UserId} accessed from {unusual.IpCount} different IP addresses in 1 hour",
                    affectedUserId: unusual.UserId);

                _logger.LogWarning("Unusual access pattern detected for user {UserId} from {IpCount} different IPs",
                    unusual.UserId, unusual.IpCount);
            }
        }

        private async Task DetectDataExfiltrationAttemptsAsync(ApplicationDbContext context)
        {
            var threshold = DateTimeOffset.UtcNow.AddMinutes(-30);

            // Detect excessive document downloads
            var excessiveDownloads = await context.SecurityAuditLogs
                .Where(log => log.EventType == "DOCUMENT_ACCESS" && 
                             log.Success && 
                             log.Timestamp > threshold &&
                             !string.IsNullOrEmpty(log.UserId))
                .GroupBy(log => log.UserId)
                .Where(group => group.Count() > 20)
                .Select(group => new { UserId = group.Key, Count = group.Count() })
                .ToListAsync();

            foreach (var excessive in excessiveDownloads)
            {
                await CreateSecurityIncidentAsync(context,
                    "POTENTIAL_DATA_EXFILTRATION",
                    SecurityIncidentSeverity.Critical,
                    $"User {excessive.UserId} accessed {excessive.Count} documents in 30 minutes - potential data exfiltration",
                    affectedUserId: excessive.UserId);

                _logger.LogCritical("Potential data exfiltration detected for user {UserId} with {Count} document accesses",
                    excessive.UserId, excessive.Count);
            }
        }

        private async Task CleanupExpiredLockoutsAsync(ApplicationDbContext context)
        {
            var expiredLockouts = await context.Users
                .Where(u => u.LockedUntil.HasValue && u.LockedUntil < DateTimeOffset.UtcNow)
                .ToListAsync();

            foreach (var user in expiredLockouts)
            {
                user.LockedUntil = null;
                user.FailedLoginAttempts = 0;
                user.UpdatedAt = DateTimeOffset.UtcNow;
            }

            if (expiredLockouts.Any())
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} expired user lockouts", expiredLockouts.Count);
            }
        }

        private async Task CreateSecurityIncidentAsync(ApplicationDbContext context, 
            string incidentType, 
            SecurityIncidentSeverity severity, 
            string description, 
            string? affectedUserId = null, 
            string? sourceIp = null)
        {
            var incident = new SecurityIncidentRecord
            {
                IncidentType = incidentType,
                Severity = severity,
                Description = description,
                AffectedUserId = affectedUserId,
                SourceIpAddress = sourceIp,
                Status = "Open",
                AutomaticResponse = "Logged and monitoring",
                DetectedAt = DateTimeOffset.UtcNow
            };

            context.SecurityIncidents.Add(incident);
            await context.SaveChangesAsync();
        }
    }
}