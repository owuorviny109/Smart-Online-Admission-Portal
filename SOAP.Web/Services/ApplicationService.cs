using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;
using SOAP.Web.Models.Entities;
using SOAP.Web.Models.DTOs;
using SOAP.Web.Services.Interfaces;
using SOAP.Web.Models;

namespace SOAP.Web.Services
{
    /// <summary>
    /// Application service implementing business logic for student applications
    /// Demonstrates: SRP, DIP, Encapsulation
    /// </summary>
    public class ApplicationService : IApplicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApplicationService> _logger;
        private readonly INotificationService _notificationService;
        private readonly ISecurityAuditService _auditService;

        public ApplicationService(
            ApplicationDbContext context,
            ILogger<ApplicationService> logger,
            INotificationService notificationService,
            ISecurityAuditService auditService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<ApplicationDto?> GetApplicationByIdAsync(int id)
        {
            var application = await _context.Applications
                .Include(a => a.School)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == id);

            return application == null ? null : MapToDto(application);
        }

        public async Task<ApplicationDto?> GetApplicationByKcpeNumberAsync(string kcpeNumber)
        {
            var application = await _context.Applications
                .Include(a => a.School)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.KcpeIndexNumber == kcpeNumber);

            return application == null ? null : MapToDto(application);
        }

        public async Task<List<ApplicationDto>> GetApplicationsBySchoolAsync(int schoolId)
        {
            var applications = await _context.Applications
                .Include(a => a.School)
                .Include(a => a.Documents)
                .Where(a => a.SchoolId == schoolId)
                .ToListAsync();

            return applications.Select(MapToDto).ToList();
        }

        public async Task<ApplicationDto> CreateApplicationAsync(ApplicationDto applicationDto)
        {
            var application = MapToEntity(applicationDto);
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
            return MapToDto(application);
        }

        public async Task<ApplicationDto> UpdateApplicationAsync(ApplicationDto applicationDto)
        {
            var application = await _context.Applications.FindAsync(applicationDto.Id);
            if (application == null) throw new ArgumentException("Application not found");

            // Update properties
            application.StudentName = applicationDto.StudentName;
            application.StudentAge = applicationDto.StudentAge;
            application.ParentPhone = applicationDto.ParentPhone;
            application.ParentName = applicationDto.ParentName;
            application.EmergencyContact = applicationDto.EmergencyContact;
            application.EmergencyName = applicationDto.EmergencyName;
            application.HomeAddress = applicationDto.HomeAddress;
            application.BoardingStatus = applicationDto.BoardingStatus;
            application.MedicalConditions = applicationDto.MedicalConditions;
            application.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return MapToDto(application);
        }

        public async Task<bool> DeleteApplicationAsync(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null) return false;

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveApplicationAsync(int id, string adminComments)
        {
            try
            {
                var application = await _context.Applications
                    .Include(a => a.School)
                    .FirstOrDefaultAsync(a => a.Id == id);
                
                if (application == null)
                {
                    _logger.LogWarning("Application not found for approval: {ApplicationId}", id);
                    return false;
                }

                // Update application status
                application.Status = "Approved";
                application.AdmissionCode = GenerateAdmissionCode();
                application.ReviewedAt = DateTimeOffset.UtcNow;
                application.UpdatedAt = DateTimeOffset.UtcNow;

                await _context.SaveChangesAsync();

                // Send approval notification
                await SendApprovalNotificationAsync(application);

                // Log security event
                await LogApplicationEventAsync("APPLICATION_APPROVED", true, id.ToString());

                _logger.LogInformation("Application {ApplicationId} approved successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to approve application {ApplicationId}", id);
                await LogApplicationEventAsync("APPLICATION_APPROVAL_FAILED", false, ex.Message);
                return false;
            }
        }

        public async Task<bool> RejectApplicationAsync(int id, string reason)
        {
            try
            {
                var application = await _context.Applications
                    .Include(a => a.School)
                    .FirstOrDefaultAsync(a => a.Id == id);
                
                if (application == null)
                {
                    _logger.LogWarning("Application not found for rejection: {ApplicationId}", id);
                    return false;
                }

                // Update application status
                application.Status = "Rejected";
                application.ReviewedAt = DateTimeOffset.UtcNow;
                application.UpdatedAt = DateTimeOffset.UtcNow;

                await _context.SaveChangesAsync();

                // Send rejection notification
                await SendRejectionNotificationAsync(application, reason);

                // Log security event
                await LogApplicationEventAsync("APPLICATION_REJECTED", true, $"ID: {id}, Reason: {reason}");

                _logger.LogInformation("Application {ApplicationId} rejected successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reject application {ApplicationId}", id);
                await LogApplicationEventAsync("APPLICATION_REJECTION_FAILED", false, ex.Message);
                return false;
            }
        }

        public async Task<bool> VerifyKcpeNumberAsync(string kcpeNumber, int schoolId)
        {
            return await _context.SchoolStudents
                .AnyAsync(ss => ss.KcpeIndexNumber == kcpeNumber && ss.SchoolId == schoolId);
        }

        private ApplicationDto MapToDto(Application application)
        {
            return new ApplicationDto
            {
                Id = application.Id,
                KcpeIndexNumber = application.KcpeIndexNumber,
                StudentName = application.StudentName,
                StudentAge = application.StudentAge,
                ParentPhone = application.ParentPhone,
                ParentName = application.ParentName,
                EmergencyContact = application.EmergencyContact,
                EmergencyName = application.EmergencyName,
                HomeAddress = application.HomeAddress,
                BoardingStatus = application.BoardingStatus,
                MedicalConditions = application.MedicalConditions,
                Status = application.Status,
                AdmissionCode = application.AdmissionCode,
                CheckedIn = application.CheckedIn,
                CreatedAt = application.CreatedAt,
                UpdatedAt = application.UpdatedAt,
                School = application.School == null ? null : new SchoolDto
                {
                    Id = application.School.Id,
                    Name = application.School.Name,
                    Code = application.School.Code,
                    County = application.School.County
                },
                Documents = application.Documents?.Select(d => new DocumentDto
                {
                    Id = d.Id,
                    ApplicationId = d.ApplicationId,
                    DocumentType = d.DocumentType,
                    FileName = d.FileName,
                    FilePath = d.FilePath,
                    FileSize = d.FileSize,
                    ContentType = d.ContentType,
                    UploadStatus = d.UploadStatus,
                    AdminFeedback = d.AdminFeedback,
                    CreatedAt = d.CreatedAt
                }).ToList() ?? new List<DocumentDto>()
            };
        }

        private Application MapToEntity(ApplicationDto dto)
        {
            return new Application
            {
                KcpeIndexNumber = dto.KcpeIndexNumber,
                StudentName = dto.StudentName,
                StudentAge = dto.StudentAge,
                ParentPhone = dto.ParentPhone,
                ParentName = dto.ParentName,
                EmergencyContact = dto.EmergencyContact,
                EmergencyName = dto.EmergencyName,
                HomeAddress = dto.HomeAddress,
                BoardingStatus = dto.BoardingStatus,
                MedicalConditions = dto.MedicalConditions,
                SchoolId = dto.School?.Id ?? 0,
                Status = dto.Status,
                AdmissionCode = dto.AdmissionCode,
                CheckedIn = dto.CheckedIn
            };
        }

        /// <summary>
        /// Generates a unique admission code
        /// Encapsulation: Private method for code generation
        /// </summary>
        private string GenerateAdmissionCode()
        {
            var year = DateTime.Now.Year.ToString()[2..];
            var randomNumber = Random.Shared.Next(100000, 999999);
            return $"{year}{randomNumber}";
        }

        /// <summary>
        /// Sends approval notification to parent
        /// Encapsulation: Private method for notification logic
        /// </summary>
        private async Task SendApprovalNotificationAsync(Application application)
        {
            try
            {
                var message = $"Congratulations! Your application for {application.School?.Name} has been approved. " +
                             $"Your admission code is: {application.AdmissionCode}";

                var context = new NotificationContext
                {
                    Subject = "Application Approved - SOAP",
                    Parameters = new Dictionary<string, object>
                    {
                        ["StudentName"] = application.StudentName,
                        ["SchoolName"] = application.School?.Name ?? "School",
                        ["AdmissionCode"] = application.AdmissionCode ?? ""
                    }
                };

                await _notificationService.SendNotificationAsync(
                    NotificationType.Sms,
                    application.ParentPhone,
                    message,
                    context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send approval notification for application {ApplicationId}", application.Id);
            }
        }

        /// <summary>
        /// Sends rejection notification to parent
        /// Encapsulation: Private method for notification logic
        /// </summary>
        private async Task SendRejectionNotificationAsync(Application application, string reason)
        {
            try
            {
                var message = $"We regret to inform you that your application for {application.School?.Name} has been rejected. " +
                             $"Reason: {reason}. Please contact the school for more information.";

                var context = new NotificationContext
                {
                    Subject = "Application Status - SOAP",
                    Parameters = new Dictionary<string, object>
                    {
                        ["StudentName"] = application.StudentName,
                        ["SchoolName"] = application.School?.Name ?? "School",
                        ["Reason"] = reason
                    }
                };

                await _notificationService.SendNotificationAsync(
                    NotificationType.Sms,
                    application.ParentPhone,
                    message,
                    context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send rejection notification for application {ApplicationId}", application.Id);
            }
        }

        /// <summary>
        /// Logs application-related security events
        /// Encapsulation: Private method for audit logging
        /// </summary>
        private async Task LogApplicationEventAsync(string eventType, bool success, string details)
        {
            try
            {
                await _auditService.LogSecurityEventAsync(new SecurityEvent
                {
                    EventType = eventType,
                    Success = success,
                    ResourceAccessed = "ApplicationService",
                    ActionPerformed = eventType,
                    AdditionalData = new Dictionary<string, object> { ["details"] = details }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log application event: {EventType}", eventType);
            }
        }
    }
}