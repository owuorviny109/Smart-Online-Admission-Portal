using SOAP.Web.Models.Entities;
using SOAP.Web.Models.DTOs;

namespace SOAP.Web.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentDto?> GetDocumentByIdAsync(int id);
        Task<List<DocumentDto>> GetDocumentsByApplicationIdAsync(int applicationId);
        Task<DocumentDto> UploadDocumentAsync(int applicationId, IFormFile file, string documentType);
        Task<bool> DeleteDocumentAsync(int id);
        Task<bool> VerifyDocumentAsync(int id, string status, string? feedback = null);
        Task<byte[]> GetDocumentContentAsync(int id);
        Task<bool> ValidateDocumentAsync(IFormFile file);
    }
}