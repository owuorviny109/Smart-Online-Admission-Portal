using SOAP.Web.Services.Interfaces;
using SOAP.Web.Models;
using SOAP.Web.Configuration;
using Microsoft.Extensions.Options;

namespace SOAP.Web.Services.Strategies
{
    /// <summary>
    /// SMS notification strategy implementation
    /// Demonstrates: Strategy Pattern, SRP, Encapsulation
    /// </summary>
    public class SmsNotificationStrategy : INotificationStrategy
    {
        private readonly ISmsService _smsService;
        private readonly ILogger<SmsNotificationStrategy> _logger;
        private readonly SmsConfig _smsConfig;

        public NotificationType NotificationType => NotificationType.Sms;
        public int Priority => 1; // SMS has high priority for urgent notifications

        public SmsNotificationStrategy(
            ISmsService smsService, 
            ILogger<SmsNotificationStrategy> logger,
            IOptions<SmsConfig> smsConfig)
        {
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _smsConfig = smsConfig.Value ?? throw new ArgumentNullException(nameof(smsConfig));
        }

        public bool CanHandle(NotificationType type)
        {
            return type == NotificationType.Sms;
        }

        public async Task<NotificationResult> SendAsync(string recipient, string message, NotificationContext context)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(recipient))
                {
                    return NotificationResult.FailureResult("Recipient phone number is required", NotificationType.Sms);
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    return NotificationResult.FailureResult("Message content is required", NotificationType.Sms);
                }

                // Format phone number
                var formattedPhone = FormatPhoneNumber(recipient);
                if (string.IsNullOrEmpty(formattedPhone))
                {
                    return NotificationResult.FailureResult("Invalid phone number format", NotificationType.Sms);
                }

                // Apply message template if specified
                var finalMessage = ApplyTemplate(message, context);

                // Send SMS using the SMS service
                var smsResult = await _smsService.SendSmsAsync(formattedPhone, finalMessage);

                if (smsResult.Success)
                {
                    _logger.LogInformation("SMS sent successfully to {Phone} with message ID {MessageId}", 
                        MaskPhoneNumber(formattedPhone), smsResult.MessageId);

                    return NotificationResult.SuccessResult(
                        smsResult.MessageId ?? Guid.NewGuid().ToString(),
                        NotificationType.Sms,
                        CalculateCost(finalMessage));
                }
                else
                {
                    _logger.LogWarning("Failed to send SMS to {Phone}: {Error}", 
                        MaskPhoneNumber(formattedPhone), smsResult.ErrorMessage);

                    return NotificationResult.FailureResult(
                        smsResult.ErrorMessage ?? "Unknown SMS sending error",
                        NotificationType.Sms);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending SMS to {Phone}", MaskPhoneNumber(recipient));
                return NotificationResult.FailureResult($"SMS sending failed: {ex.Message}", NotificationType.Sms);
            }
        }

        /// <summary>
        /// Formats phone number to international format
        /// Encapsulation: Private method for internal logic
        /// </summary>
        private string FormatPhoneNumber(string phoneNumber)
        {
            // Remove all non-digit characters
            var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Handle Kenyan phone numbers
            if (digitsOnly.StartsWith("254"))
            {
                return $"+{digitsOnly}";
            }
            else if (digitsOnly.StartsWith("0") && digitsOnly.Length == 10)
            {
                return $"+254{digitsOnly.Substring(1)}";
            }
            else if (digitsOnly.Length == 9)
            {
                return $"+254{digitsOnly}";
            }

            // Return original if format is unclear
            return phoneNumber.StartsWith("+") ? phoneNumber : $"+{digitsOnly}";
        }

        /// <summary>
        /// Applies message template with context parameters
        /// Encapsulation: Private method for template processing
        /// </summary>
        private string ApplyTemplate(string message, NotificationContext context)
        {
            if (string.IsNullOrEmpty(context.TemplateId) || !context.Parameters.Any())
            {
                return message;
            }

            var result = message;
            foreach (var parameter in context.Parameters)
            {
                var placeholder = $"{{{parameter.Key}}}";
                result = result.Replace(placeholder, parameter.Value?.ToString() ?? "");
            }

            return result;
        }

        /// <summary>
        /// Calculates SMS cost based on message length
        /// Encapsulation: Private method for cost calculation
        /// </summary>
        private decimal CalculateCost(string message)
        {
            // SMS cost calculation (160 characters per SMS unit)
            var smsUnits = Math.Ceiling((double)message.Length / 160);
            return (decimal)smsUnits * _smsConfig.CostPerSms;
        }

        /// <summary>
        /// Masks phone number for logging (privacy protection)
        /// Encapsulation: Private method for data protection
        /// </summary>
        private string MaskPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
                return "****";

            return phoneNumber.Substring(0, 3) + "****" + phoneNumber.Substring(phoneNumber.Length - 2);
        }
    }
}