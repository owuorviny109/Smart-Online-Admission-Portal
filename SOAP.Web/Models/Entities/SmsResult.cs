namespace SOAP.Web.Models.Entities
{
    /// <summary>
    /// Simple SMS result model for compatibility
    /// </summary>
    public class SmsResult
    {
        public bool Success { get; set; }
        public string? MessageId { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Simple Email result model for compatibility
    /// </summary>
    public class EmailResult
    {
        public bool Success { get; set; }
        public string? MessageId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}