namespace SOAP.Web.Models
{
    public class SecurityEvent
    {
        public string EventType { get; set; } = "";
        public string? UserId { get; set; }
        public string? UserRole { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ResourceAccessed { get; set; }
        public string? ActionPerformed { get; set; }
        public bool Success { get; set; }
        public string? FailureReason { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}