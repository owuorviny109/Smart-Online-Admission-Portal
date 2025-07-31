using SOAP.Web.Models.Entities;

namespace SOAP.Web.Services.Interfaces
{
    public interface ISecurityAuditService
    {
        Task LogSecurityEventAsync(SecurityEvent securityEvent);
        Task LogLoginAttemptAsync(string phoneNumber, bool success, string? failureReason = null);
        Task LogDataAccessAsync(string userId, string resource, string action, bool success);
        Task LogFileUploadAsync(string userId, string fileName, string documentType, bool success);
        Task<List<SecurityAuditLog>> GetSecurityEventsAsync(DateTimeOffset from, DateTimeOffset to);
        Task<SecurityMetrics> GetSecurityMetricsAsync();
    }

    public class SecurityEvent
    {
        public string EventType { get; set; } = "";
        public string UserId { get; set; } = "";
        public string UserRole { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public string UserAgent { get; set; } = "";
        public string ResourceAccessed { get; set; } = "";
        public string ActionPerformed { get; set; } = "";
        public bool Success { get; set; }
        public string FailureReason { get; set; } = "";
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    public class SecurityMetrics
    {
        public int TotalLoginAttempts { get; set; }
        public int FailedLoginAttempts { get; set; }
        public int SuccessfulLogins { get; set; }
        public int UnauthorizedAccessAttempts { get; set; }
        public int SecurityIncidents { get; set; }
        public decimal SecurityScore { get; set; }
    }
} 