namespace SOAP.Web.Models
{
    /// <summary>
    /// Enumeration of notification types
    /// </summary>
    public enum NotificationType
    {
        Sms,
        Email,
        Push,
        InApp
    }

    /// <summary>
    /// Context information for notifications
    /// Encapsulation: Contains all notification metadata
    /// </summary>
    public class NotificationContext
    {
        public string Subject { get; set; } = "";
        public string TemplateId { get; set; } = "";
        public Dictionary<string, object> Parameters { get; set; } = new();
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public bool IsUrgent { get; set; } = false;
        public string? UserId { get; set; }
        public string? ApplicationId { get; set; }
    }

    /// <summary>
    /// Priority levels for notifications
    /// </summary>
    public enum NotificationPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// Result of notification sending operation
    /// Encapsulation: Contains operation result and metadata
    /// </summary>
    public class NotificationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? MessageId { get; set; }
        public DateTime SentAt { get; set; }
        public NotificationType Type { get; set; }
        public decimal? Cost { get; set; }

        public static NotificationResult SuccessResult(string messageId, NotificationType type, decimal? cost = null)
        {
            return new NotificationResult
            {
                Success = true,
                MessageId = messageId,
                Type = type,
                SentAt = DateTime.UtcNow,
                Cost = cost
            };
        }

        public static NotificationResult FailureResult(string errorMessage, NotificationType type)
        {
            return new NotificationResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                Type = type,
                SentAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Bulk notification request
    /// </summary>
    public class BulkNotificationRequest
    {
        public List<string> Recipients { get; set; } = new();
        public string Message { get; set; } = "";
        public NotificationContext Context { get; set; } = new();
        public NotificationType Type { get; set; }
    }

    /// <summary>
    /// Result of bulk notification operation
    /// </summary>
    public class BulkNotificationResult
    {
        public int TotalRecipients { get; set; }
        public int SuccessfulSends { get; set; }
        public int FailedSends { get; set; }
        public List<NotificationResult> Results { get; set; } = new();
        public decimal? TotalCost { get; set; }

        public bool IsCompleteSuccess => FailedSends == 0;
        public double SuccessRate => TotalRecipients > 0 ? (double)SuccessfulSends / TotalRecipients * 100 : 0;
    }
}