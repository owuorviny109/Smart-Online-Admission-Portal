using SOAP.Web.Models;

namespace SOAP.Web.Services.Interfaces
{
    /// <summary>
    /// Strategy pattern interface for different notification methods
    /// Demonstrates: Abstraction, OCP (Open/Closed Principle)
    /// </summary>
    public interface INotificationStrategy
    {
        /// <summary>
        /// Sends notification using specific strategy
        /// </summary>
        Task<NotificationResult> SendAsync(string recipient, string message, NotificationContext context);
        
        /// <summary>
        /// Determines if this strategy can handle the notification type
        /// </summary>
        bool CanHandle(NotificationType type);
        
        /// <summary>
        /// Gets the notification type this strategy handles
        /// </summary>
        NotificationType NotificationType { get; }
        
        /// <summary>
        /// Gets the priority of this strategy (higher number = higher priority)
        /// </summary>
        int Priority { get; }
    }
}