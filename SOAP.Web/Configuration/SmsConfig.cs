namespace SOAP.Web.Configuration
{
    public class SmsConfig
    {
        public string Provider { get; set; } = "AfricasTalking";
        public string ApiKey { get; set; } = "";
        public string Username { get; set; } = "";
        public string SenderId { get; set; } = "SOAP";
        public bool DeliveryReports { get; set; } = true;
        public decimal CostPerSms { get; set; } = 0.05m; // Cost per SMS unit
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;
        public int TimeoutSeconds { get; set; } = 30;
        public string BaseUrl { get; set; } = "https://api.africastalking.com";
    }
}