namespace SOAP.Web.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendApplicationConfirmationAsync(string email, string studentName, string applicationId);
        Task<bool> SendApplicationStatusUpdateAsync(string email, string studentName, string status);
        Task<bool> SendAdmissionSlipAsync(string email, string studentName, byte[] admissionSlipPdf);
        Task<bool> SendPasswordResetAsync(string email, string resetToken);
    }
}