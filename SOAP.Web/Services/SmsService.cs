using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;
using SOAP.Web.Models.Entities;
using SOAP.Web.Services.Interfaces;

namespace SOAP.Web.Services
{
    public class SmsService : ISmsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, string> _otpStorage = new(); // In production, use Redis or database

        public SmsService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message, int? applicationId = null)
        {
            try
            {
                // TODO: Integrate with Africa's Talking SMS API
                // For now, just log the SMS
                var smsLog = new SmsLog
                {
                    PhoneNumber = phoneNumber,
                    MessageType = "Outgoing",
                    Content = message,
                    Status = "Sent",
                    ApplicationId = applicationId
                };

                _context.SmsLogs.Add(smsLog);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendOtpAsync(string phoneNumber)
        {
            var otp = GenerateOtp();
            _otpStorage[phoneNumber] = otp;

            var message = $"Your SOAP verification code is: {otp}. Valid for 5 minutes.";
            return await SendSmsAsync(phoneNumber, message);
        }

        public Task<bool> VerifyOtpAsync(string phoneNumber, string otp)
        {
            if (_otpStorage.TryGetValue(phoneNumber, out var storedOtp) && storedOtp == otp)
            {
                _otpStorage.Remove(phoneNumber);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public async Task<List<SmsLog>> GetSmsHistoryAsync(string phoneNumber)
        {
            return await _context.SmsLogs
                .Where(s => s.PhoneNumber == phoneNumber)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task ProcessIncomingSmsAsync(string phoneNumber, string message)
        {
            var smsLog = new SmsLog
            {
                PhoneNumber = phoneNumber,
                MessageType = "Incoming",
                Content = message,
                Status = "Received"
            };

            _context.SmsLogs.Add(smsLog);
            await _context.SaveChangesAsync();

            // TODO: Process SMS commands for form completion
        }

        public async Task<bool> SendApplicationStatusUpdateAsync(int applicationId, string status)
        {
            var application = await _context.Applications.FindAsync(applicationId);
            if (application == null) return false;

            var message = status switch
            {
                "Approved" => $"Congratulations! Your application for {application.StudentName} has been approved. Admission code: {application.AdmissionCode}",
                "Rejected" => $"Your application for {application.StudentName} has been rejected. Please contact the school for more information.",
                _ => $"Your application for {application.StudentName} status has been updated to: {status}"
            };

            return await SendSmsAsync(application.ParentPhone, message, applicationId);
        }

        public async Task<bool> SendWelcomeMessageAsync(string phoneNumber, string studentName)
        {
            var message = $"Welcome to SOAP! Your application for {studentName} has been received. You will receive updates via SMS.";
            return await SendSmsAsync(phoneNumber, message);
        }

        private string GenerateOtp()
        {
            return Random.Shared.Next(100000, 999999).ToString();
        }
    }
}