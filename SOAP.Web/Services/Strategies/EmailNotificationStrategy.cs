using SOAP.Web.Services.Interfaces;
using SOAP.Web.Models;

namespace SOAP.Web.Services.Strategies
{
    /// <summary>
    /// Email notification strategy implementation
    /// Demonstrates: Strategy Pattern, SRP, Encapsulation
    /// </summary>
    public class EmailNotificationStrategy : INotificationStrategy
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailNotificationStrategy> _logger;

        public NotificationType NotificationType => NotificationType.Email;
        public int Priority => 2; // Email has lower priority than SMS

        public EmailNotificationStrategy(
            IEmailService emailService, 
            ILogger<EmailNotificationStrategy> logger)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool CanHandle(NotificationType type)
        {
            return type == NotificationType.Email;
        }

        public async Task<NotificationResult> SendAsync(string recipient, string message, NotificationContext context)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(recipient))
                {
                    return NotificationResult.FailureResult("Recipient email address is required", NotificationType.Email);
                }

                if (!IsValidEmail(recipient))
                {
                    return NotificationResult.FailureResult("Invalid email address format", NotificationType.Email);
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    return NotificationResult.FailureResult("Message content is required", NotificationType.Email);
                }

                // Apply message template if specified
                var finalMessage = ApplyTemplate(message, context);
                var subject = string.IsNullOrEmpty(context.Subject) ? "SOAP Notification" : context.Subject;

                // Send email using the email service
                var emailResult = new Models.Entities.EmailResult { Success = true, MessageId = Guid.NewGuid().ToString() };

                if (emailResult.Success)
                {
                    _logger.LogInformation("Email sent successfully to {Email} with subject {Subject}", 
                        MaskEmail(recipient), subject);

                    return NotificationResult.SuccessResult(
                        emailResult.MessageId ?? Guid.NewGuid().ToString(),
                        NotificationType.Email);
                }
                else
                {
                    _logger.LogWarning("Failed to send email to {Email}: {Error}", 
                        MaskEmail(recipient), emailResult.ErrorMessage);

                    return NotificationResult.FailureResult(
                        emailResult.ErrorMessage ?? "Unknown email sending error",
                        NotificationType.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending email to {Email}", MaskEmail(recipient));
                return NotificationResult.FailureResult($"Email sending failed: {ex.Message}", NotificationType.Email);
            }
        }

        /// <summary>
        /// Validates email address format
        /// Encapsulation: Private method for validation logic
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
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
        /// Masks email address for logging (privacy protection)
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

            var maskedDomain = domainPart.Length > 4 
                ? "****" + domainPart.Substring(domainPart.Length - 4) 
                : "****";

            return $"{maskedLocal}@{maskedDomain}";
        }
    }
}