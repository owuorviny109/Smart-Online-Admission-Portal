using SOAP.Web.Services.Interfaces;
using SOAP.Web.Models;

namespace SOAP.Web.Services
{
    /// <summary>
    /// Unified notification service using strategy pattern
    /// Demonstrates: Strategy Pattern, DIP, SRP, OCP
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IEnumerable<INotificationStrategy> _strategies;
        private readonly ILogger<NotificationService> _logger;
        private readonly ISecurityAuditService _auditService;

        public NotificationService(
            IEnumerable<INotificationStrategy> strategies,
            ILogger<NotificationService> logger,
            ISecurityAuditService auditService)
        {
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<NotificationResult> SendNotificationAsync(
            NotificationType type, 
            string recipient, 
            string message, 
            NotificationContext? context = null)
        {
            try
            {
                // Find appropriate strategy
                var strategy = GetStrategyForType(type);
                if (strategy == null)
                {
                    var errorMessage = $"No strategy found for notification type: {type}";
                    _logger.LogWarning(errorMessage);
                    
                    await LogNotificationEventAsync("NOTIFICATION_STRATEGY_NOT_FOUND", false, type.ToString());
                    return NotificationResult.FailureResult(errorMessage, type);
                }

                // Use null object pattern for context
                context ??= new NotificationContext();

                // Send notification
                _logger.LogInformation("Sending {Type} notification to {Recipient}", type, MaskRecipient(recipient, type));
                
                var result = await strategy.SendAsync(recipient, message, context);

                // Log the result
                await LogNotificationEventAsync(
                    result.Success ? "NOTIFICATION_SENT" : "NOTIFICATION_FAILED",
                    result.Success,
                    $"Type: {type}, Success: {result.Success}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending {Type} notification to {Recipient}", 
                    type, MaskRecipient(recipient, type));
                
                await LogNotificationEventAsync("NOTIFICATION_EXCEPTION", false, ex.Message);
                return NotificationResult.FailureResult($"Notification sending failed: {ex.Message}", type);
            }
        }

        public async Task<BulkNotificationResult> SendBulkNotificationAsync(BulkNotificationRequest request)
        {
            var result = new BulkNotificationResult
            {
                TotalRecipients = request.Recipients.Count
            };

            try
            {
                _logger.LogInformation("Starting bulk {Type} notification to {Count} recipients", 
                    request.Type, request.Recipients.Count);

                // Send notifications concurrently with controlled parallelism
                var semaphore = new SemaphoreSlim(10); // Limit concurrent operations
                var tasks = request.Recipients.Select(async recipient =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        return await SendNotificationAsync(request.Type, recipient, request.Message, request.Context);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                var results = await Task.WhenAll(tasks);
                result.Results.AddRange(results);

                // Calculate statistics
                result.SuccessfulSends = results.Count(r => r.Success);
                result.FailedSends = results.Count(r => !r.Success);
                result.TotalCost = results.Where(r => r.Cost.HasValue).Sum(r => r.Cost.Value);

                _logger.LogInformation("Bulk notification completed: {Successful}/{Total} successful", 
                    result.SuccessfulSends, result.TotalRecipients);

                await LogNotificationEventAsync("BULK_NOTIFICATION_COMPLETED", result.IsCompleteSuccess,
                    $"Type: {request.Type}, Success Rate: {result.SuccessRate:F1}%");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during bulk {Type} notification", request.Type);
                await LogNotificationEventAsync("BULK_NOTIFICATION_EXCEPTION", false, ex.Message);
                throw;
            }
        }

        public IEnumerable<NotificationType> GetAvailableNotificationTypes()
        {
            return _strategies.Select(s => s.NotificationType).Distinct();
        }

        public bool IsNotificationTypeSupported(NotificationType type)
        {
            return _strategies.Any(s => s.CanHandle(type));
        }

        /// <summary>
        /// Gets the appropriate strategy for a notification type
        /// Encapsulation: Private method for strategy selection
        /// </summary>
        private INotificationStrategy? GetStrategyForType(NotificationType type)
        {
            // Get all strategies that can handle this type, ordered by priority
            return _strategies
                .Where(s => s.CanHandle(type))
                .OrderByDescending(s => s.Priority)
                .FirstOrDefault();
        }

        /// <summary>
        /// Masks recipient information for logging
        /// Encapsulation: Private method for privacy protection
        /// </summary>
        private string MaskRecipient(string recipient, NotificationType type)
        {
            if (string.IsNullOrEmpty(recipient))
                return "****";

            return type switch
            {
                NotificationType.Email => MaskEmail(recipient),
                NotificationType.Sms => MaskPhoneNumber(recipient),
                _ => "****"
            };
        }

        /// <summary>
        /// Masks email address for logging
        /// Encapsulation: Private method for data protection
        /// </summary>
        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return "****@****.***";

            var parts = email.Split('@');
            var localPart = parts[0];
            var domainPart = parts[1];

            var maskedLocal = localPart.Length > 2 
                ? localPart.Substring(0, 2) + "****" 
                : "****";

            return $"{maskedLocal}@{domainPart}";
        }

        /// <summary>
        /// Masks phone number for logging
        /// Encapsulation: Private method for data protection
        /// </summary>
        private string MaskPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
                return "****";

            return phoneNumber.Substring(0, 3) + "****" + phoneNumber.Substring(phoneNumber.Length - 2);
        }

        /// <summary>
        /// Logs notification events for security audit
        /// Encapsulation: Private method for audit logging
        /// </summary>
        private async Task LogNotificationEventAsync(string eventType, bool success, string details)
        {
            try
            {
                await _auditService.LogSecurityEventAsync(new SecurityEvent
                {
                    EventType = eventType,
                    Success = success,
                    ResourceAccessed = "NotificationService",
                    ActionPerformed = "SendNotification",
                    AdditionalData = new Dictionary<string, object> { ["details"] = details }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log notification event: {EventType}", eventType);
            }
        }
    }
}