using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;
using SOAP.Web.Models.Entities;
using SOAP.Web.Models.DTOs;
using SOAP.Web.Services.Interfaces;

namespace SOAP.Web.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        private const long MaxFileSize = 2 * 1024 * 1024; // 2MB

        public DocumentService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<DocumentDto?> GetDocumentByIdAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            return document == null ? null : MapToDto(document);
        }

        public async Task<List<DocumentDto>> GetDocumentsByApplicationIdAsync(int applicationId)
        {
            var documents = await _context.Documents
                .Where(d => d.ApplicationId == applicationId)
                .ToListAsync();

            return documents.Select(MapToDto).ToList();
        }

        public async Task<DocumentDto> UploadDocumentAsync(int applicationId, IFormFile file, string documentType)
        {
            if (!await ValidateDocumentAsync(file))
                throw new ArgumentException("Invalid file");

            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "documents");
            Directory.CreateDirectory(uploadsPath);

            var fileName = $"{applicationId}_{documentType}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new Document
            {
                ApplicationId = applicationId,
                DocumentType = documentType,
                OriginalFileName = file.FileName,
                FilePath = Path.Combine("uploads", "documents", fileName),
                FileSize = file.Length,
                ContentType = file.ContentType,
                VerificationStatus = "Pending"
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return MapToDto(document);
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return false;

            // Delete physical file
            var fullPath = Path.Combine(_environment.WebRootPath, document.FilePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyDocumentAsync(int id, string status, string? feedback = null)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return false;

            document.VerificationStatus = status;
            document.AdminFeedback = feedback;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<byte[]> GetDocumentContentAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) throw new FileNotFoundException("Document not found");

            var fullPath = Path.Combine(_environment.WebRootPath, document.FilePath);
            if (!File.Exists(fullPath)) throw new FileNotFoundException("Physical file not found");

            return await File.ReadAllBytesAsync(fullPath);
        }

        public Task<bool> ValidateDocumentAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Task.FromResult(false);

            if (file.Length > MaxFileSize)
                return Task.FromResult(false);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        private DocumentDto MapToDto(Document document)
        {
            return new DocumentDto
            {
                Id = document.Id,
                ApplicationId = document.ApplicationId,
                DocumentType = document.DocumentType,
                FileName = document.OriginalFileName,
                FilePath = document.FilePath,
                FileSize = document.FileSize,
                ContentType = document.ContentType,
                UploadStatus = document.VerificationStatus,
                AdminFeedback = document.AdminFeedback,
                CreatedAt = document.UploadedAt
            };
        }
    }
}