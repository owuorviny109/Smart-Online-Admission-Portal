using SOAP.Web.Models.Entities;
using SOAP.Web.Models.DTOs;

namespace SOAP.Web.Services.Interfaces
{
    public interface IApplicationService
    {
        Task<ApplicationDto?> GetApplicationByIdAsync(int id);
        Task<ApplicationDto?> GetApplicationByKcpeNumberAsync(string kcpeNumber);
        Task<List<ApplicationDto>> GetApplicationsBySchoolAsync(int schoolId);
        Task<ApplicationDto> CreateApplicationAsync(ApplicationDto applicationDto);
        Task<ApplicationDto> UpdateApplicationAsync(ApplicationDto applicationDto);
        Task<bool> DeleteApplicationAsync(int id);
        Task<bool> ApproveApplicationAsync(int id, string adminComments);
        Task<bool> RejectApplicationAsync(int id, string reason);
        Task<bool> VerifyKcpeNumberAsync(string kcpeNumber, int schoolId);
    }
}