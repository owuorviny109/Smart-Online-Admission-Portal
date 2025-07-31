using SOAP.Web.Models;

namespace SOAP.Web.Services.Interfaces
{
    /// <summary>
    /// Unified notification service interface
    /// Demonstrates: Abstraction, ISP (Interface Segregation)
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends a single notification using the specified type
        /// </summary>
        Task<NotificationResult> SendNotificationAsync(NotificationType type, string recipient, string message, NotificationContext? context = null);

        /// <summary>
        /// Sends bulk notifications to multiple recipients
        /// </summary>
        Task<BulkNotificationResult> SendBulkNotificationAsync(BulkNotificationRequest request);

        /// <summary>
        /// Gets available notification strategies
        /// </summary>
        IEnumerable<NotificationType> GetAvailableNotificationTypes();

        /// <summary>
        /// Checks if a notification type is supported
        /// </summary>
        bool IsNotificationTypeSupported(NotificationType type);
    }
}