using SOAP.Web.Models.Entities;

namespace SOAP.Web.Services.Interfaces
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message, int? applicationId = null);
        Task<bool> SendOtpAsync(string phoneNumber);
        Task<bool> VerifyOtpAsync(string phoneNumber, string otp);
        Task<List<SmsLog>> GetSmsHistoryAsync(string phoneNumber);
        Task ProcessIncomingSmsAsync(string phoneNumber, string message);
        Task<bool> SendApplicationStatusUpdateAsync(int applicationId, string status);
        Task<bool> SendWelcomeMessageAsync(string phoneNumber, string studentName);
    }
}