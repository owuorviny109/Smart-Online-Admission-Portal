using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;
using SOAP.Web.Models.Entities;
using SOAP.Web.Models.DTOs;
using SOAP.Web.Services.Interfaces;

namespace SOAP.Web.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly ApplicationDbContext _context;

        public ApplicationService(ApplicationDbContext context)
        {
            _context = context;
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
            var application = await _context.Applications.FindAsync(id);
            if (application == null) return false;

            application.Status = "Approved";
            application.AdmissionCode = GenerateAdmissionCode();
            application.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectApplicationAsync(int id, string reason)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null) return false;

            application.Status = "Rejected";
            application.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
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

        private string GenerateAdmissionCode()
        {
            return DateTime.Now.Year.ToString()[2..] + Random.Shared.Next(100000, 999999).ToString();
        }
    }
}