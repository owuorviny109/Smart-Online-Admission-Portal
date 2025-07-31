namespace SOAP.Web.Services.Interfaces
{
    public interface IRateLimitingService
    {
        Task<bool> IsRequestAllowedAsync(string clientId, string endpoint, int maxRequests, TimeSpan window);
        Task<bool> IsLoginAllowedAsync(string phoneNumber);
        Task<bool> IsOtpRequestAllowedAsync(string phoneNumber);
        Task<bool> IsUploadAllowedAsync(string userId);
        Task IncrementRequestCountAsync(string clientId, string endpoint);
        Task ResetRequestCountAsync(string clientId, string endpoint);
    }
} 