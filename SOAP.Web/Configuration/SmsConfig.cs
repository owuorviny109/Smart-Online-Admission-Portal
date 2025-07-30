namespace SOAP.Web.Configuration
{
    public class SmsConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string SenderId { get; set; } = "SOAP";
        public string BaseUrl { get; set; } = "https://api.africastalking.com";
        public bool EnableSms { get; set; } = true;
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
        public int OtpExpiryMinutes { get; set; } = 5;
        public int OtpLength { get; set; } = 6;
        
        public class MessageTemplates
        {
            public string OtpMessage { get; set; } = "Your SOAP verification code is: {0}. Valid for {1} minutes.";
            public string WelcomeMessage { get; set; } = "Welcome to SOAP! Your application for {0} has been received.";
            public string ApplicationApproved { get; set; } = "Congratulations! Your application for {0} has been approved. Admission code: {1}";
            public string ApplicationRejected { get; set; } = "Your application for {0} has been rejected. Please contact the school for more information.";
            public string DocumentsRequired { get; set; } = "Please upload the required documents for {0}'s application. Visit: {1}";
        }
        
        public MessageTemplates Templates { get; set; } = new MessageTemplates();
    }
}