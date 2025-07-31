using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;
using SOAP.Web.Models.Entities;
using SOAP.Web.Services.Interfaces;
using System.Text.Json;

namespace SOAP.Web.Services
{
    public class SecurityAuditService : ISecurityAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SecurityAuditService> _logger;

        public SecurityAuditService(ApplicationDbContext context, ILogger<SecurityAuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogSecurityEventAsync(SecurityEvent securityEvent)
        {
            try
            {
                var auditLog = new SecurityAuditLog
                {
                    EventType = securityEvent.EventType,
                    UserId = securityEvent.UserId,
                    UserRole = securityEvent.UserRole,
                    IpAddress = securityEvent.IpAddress,
                    UserAgent = securityEvent.UserAgent,
                    ResourceAccessed = securityEvent.ResourceAccessed,
                    ActionPerformed = securityEvent.ActionPerformed,
                    Success = securityEvent.Success,
                    FailureReason = securityEvent.FailureReason,
                    AdditionalData = JsonSerializer.Serialize(securityEvent.AdditionalData),
                    Timestamp = DateTimeOffset.UtcNow
                };

                _context.SecurityAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                // Also log to structured logging system
                _logger.LogInformation("Security Event: {EventType} by {UserId} from {IpAddress} - {Success}",
                    securityEvent.EventType, securityEvent.UserId, securityEvent.IpAddress,
                    securityEvent.Success ? "SUCCESS" : "FAILED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security event");
            }
        }

        public async Task LogLoginAttemptAsync(string phoneNumber, bool success, string? failureReason = null)
        {
            var securityEvent = new SecurityEvent
            {
                EventType = success ? "LOGIN_SUCCESS" : "LOGIN_FAILED",
                UserId = phoneNumber,
                Success = success,
                FailureReason = failureReason ?? "",
                ActionPerformed = "LOGIN_ATTEMPT"
            };

            await LogSecurityEventAsync(securityEvent);
        }

        public async Task LogDataAccessAsync(string userId, string resource, string action, bool success)
        {
            var securityEvent = new SecurityEvent
            {
                EventType = success ? "DATA_ACCESS_SUCCESS" : "DATA_ACCESS_FAILED",
                UserId = userId,
                ResourceAccessed = resource,
                ActionPerformed = action,
                Success = success
            };

            await LogSecurityEventAsync(securityEvent);
        }

        public async Task LogFileUploadAsync(string userId, string fileName, string documentType, bool success)
        {
            var securityEvent = new SecurityEvent
            {
                EventType = success ? "FILE_UPLOAD_SUCCESS" : "FILE_UPLOAD_FAILED",
                UserId = userId,
                ResourceAccessed = fileName,
                ActionPerformed = $"UPLOAD_{documentType}",
                Success = success,
                AdditionalData = new Dictionary<string, object>
                {
                    { "DocumentType", documentType },
                    { "FileName", fileName }
                }
            };

            await LogSecurityEventAsync(securityEvent);
        }

        public async Task<List<SecurityAuditLog>> GetSecurityEventsAsync(DateTimeOffset from, DateTimeOffset to)
        {
            return await _context.SecurityAuditLogs
                .Where(log => log.Timestamp >= from && log.Timestamp <= to)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        public async Task<SecurityMetrics> GetSecurityMetricsAsync()
        {
            var last24Hours = DateTimeOffset.UtcNow.AddHours(-24);

            var metrics = new SecurityMetrics
            {
                TotalLoginAttempts = await _context.SecurityAuditLogs
                    .Where(log => log.EventType.StartsWith("LOGIN_") && log.Timestamp >= last24Hours)
                    .CountAsync(),

                FailedLoginAttempts = await _context.SecurityAuditLogs
                    .Where(log => log.EventType == "LOGIN_FAILED" && log.Timestamp >= last24Hours)
                    .CountAsync(),

                SuccessfulLogins = await _context.SecurityAuditLogs
                    .Where(log => log.EventType == "LOGIN_SUCCESS" && log.Timestamp >= last24Hours)
                    .CountAsync(),

                UnauthorizedAccessAttempts = await _context.SecurityAuditLogs
                    .Where(log => log.EventType == "DATA_ACCESS_FAILED" && log.Timestamp >= last24Hours)
                    .CountAsync(),

                SecurityIncidents = await _context.SecurityAuditLogs
                    .Where(log => !log.Success && log.Timestamp >= last24Hours)
                    .CountAsync()
            };

            // Calculate security score
            if (metrics.TotalLoginAttempts > 0)
            {
                var failureRate = (decimal)metrics.FailedLoginAttempts / metrics.TotalLoginAttempts;
                metrics.SecurityScore = Math.Max(0, 100 - (failureRate * 100));
            }
            else
            {
                metrics.SecurityScore = 100;
            }

            return metrics;
        }
    }
} 