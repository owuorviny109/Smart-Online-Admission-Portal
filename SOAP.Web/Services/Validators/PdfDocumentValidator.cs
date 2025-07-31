using SOAP.Web.Services.Interfaces;
using SOAP.Web.Models;
using System.Security.Cryptography;

namespace SOAP.Web.Services.Validators
{
    /// <summary>
    /// PDF document validator implementation
    /// Demonstrates: Strategy Pattern, SRP, Encapsulation
    /// </summary>
    public class PdfDocumentValidator : IDocumentValidator
    {
        private readonly ILogger<PdfDocumentValidator> _logger;
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
        private static readonly byte[] PdfSignature = { 0x25, 0x50, 0x44, 0x46 }; // %PDF

        public IEnumerable<DocumentType> SupportedTypes => new[]
        {
            DocumentType.BirthCertificate,
            DocumentType.KcpeCertificate,
            DocumentType.ParentId,
            DocumentType.MedicalForm,
            DocumentType.TransferLetter,
            DocumentType.Other
        };

        public int Priority => 1;

        public PdfDocumentValidator(ILogger<PdfDocumentValidator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool CanValidate(DocumentType documentType)
        {
            return SupportedTypes.Contains(documentType);
        }

        public async Task<DocumentValidationResult> ValidateAsync(IFormFile file, DocumentType documentType)
        {
            try
            {
                // Basic file validation
                var basicValidation = ValidateBasicProperties(file);
                if (!basicValidation.IsValid)
                    return basicValidation;

                // File signature validation
                var signatureValidation = await ValidateFileSignatureAsync(file);
                if (!signatureValidation.IsValid)
                    return signatureValidation;

                // Content validation
                var contentValidation = await ValidateContentAsync(file);
                if (!contentValidation.IsValid)
                    return contentValidation;

                // Extract metadata
                var metadata = await ExtractMetadataAsync(file);
                var fileHash = await CalculateFileHashAsync(file);

                _logger.LogInformation("PDF validation successful for file: {FileName}", file.FileName);

                return DocumentValidationResult.Success(metadata, fileHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF validation failed for file: {FileName}", file.FileName);
                return DocumentValidationResult.Failure($"PDF validation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates basic file properties
        /// Encapsulation: Private method for basic validation
        /// </summary>
        private DocumentValidationResult ValidateBasicProperties(IFormFile file)
        {
            var errors = new List<string>();

            // Check if file exists
            if (file == null || file.Length == 0)
            {
                errors.Add("File is empty or not provided");
            }

            // Check file size
            if (file != null && file.Length > MaxFileSizeBytes)
            {
                errors.Add($"File size exceeds maximum limit of {MaxFileSizeBytes / (1024 * 1024)}MB");
            }

            // Check file extension
            if (file != null && !Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("File must have .pdf extension");
            }

            // Check content type
            if (file != null && !file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("File content type must be application/pdf");
            }

            return errors.Any() 
                ? DocumentValidationResult.Failure(errors.ToArray())
                : DocumentValidationResult.Success();
        }

        /// <summary>
        /// Validates PDF file signature (magic bytes)
        /// Encapsulation: Private method for signature validation
        /// </summary>
        private async Task<DocumentValidationResult> ValidateFileSignatureAsync(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var buffer = new byte[PdfSignature.Length];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead < PdfSignature.Length)
                {
                    return DocumentValidationResult.Failure("File is too small to be a valid PDF");
                }

                if (!buffer.SequenceEqual(PdfSignature))
                {
                    return DocumentValidationResult.Failure("File does not have a valid PDF signature");
                }

                return DocumentValidationResult.Success();
            }
            catch (Exception ex)
            {
                return DocumentValidationResult.Failure($"Failed to validate PDF signature: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates PDF content structure
        /// Encapsulation: Private method for content validation
        /// </summary>
        private async Task<DocumentValidationResult> ValidateContentAsync(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);

                // Read the entire content
                var content = await reader.ReadToEndAsync();

                // Check for PDF structure markers
                if (!content.Contains("%%EOF"))
                {
                    return DocumentValidationResult.Warning("PDF may be corrupted - missing EOF marker");
                }

                // Check for suspicious content (basic security check)
                var suspiciousPatterns = new[] { "<script", "javascript:", "vbscript:" };
                foreach (var pattern in suspiciousPatterns)
                {
                    if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        return DocumentValidationResult.Failure("PDF contains potentially malicious content");
                    }
                }

                return DocumentValidationResult.Success();
            }
            catch (Exception ex)
            {
                return DocumentValidationResult.Failure($"Failed to validate PDF content: {ex.Message}");
            }
        }

        /// <summary>
        /// Extracts metadata from PDF file
        /// Encapsulation: Private method for metadata extraction
        /// </summary>
        private async Task<DocumentMetadata> ExtractMetadataAsync(IFormFile file)
        {
            try
            {
                var metadata = new DocumentMetadata
                {
                    FileName = file.FileName,
                    FileSize = file.Length,
                    ContentType = file.ContentType,
                    FileExtension = Path.GetExtension(file.FileName),
                    CreatedDate = DateTime.UtcNow
                };

                // Basic PDF metadata extraction (simplified)
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                var content = await reader.ReadToEndAsync();

                // Extract title if present
                var titleMatch = System.Text.RegularExpressions.Regex.Match(content, @"/Title\s*\(([^)]+)\)");
                if (titleMatch.Success)
                {
                    metadata.Title = titleMatch.Groups[1].Value;
                }

                // Extract author if present
                var authorMatch = System.Text.RegularExpressions.Regex.Match(content, @"/Author\s*\(([^)]+)\)");
                if (authorMatch.Success)
                {
                    metadata.Author = authorMatch.Groups[1].Value;
                }

                // Estimate page count (very basic)
                var pageMatches = System.Text.RegularExpressions.Regex.Matches(content, @"/Type\s*/Page\b");
                metadata.PageCount = pageMatches.Count;

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract PDF metadata for file: {FileName}", file.FileName);
                
                return new DocumentMetadata
                {
                    FileName = file.FileName,
                    FileSize = file.Length,
                    ContentType = file.ContentType,
                    FileExtension = Path.GetExtension(file.FileName),
                    CreatedDate = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Calculates SHA-256 hash of the file
        /// Encapsulation: Private method for hash calculation
        /// </summary>
        private async Task<string> CalculateFileHashAsync(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var sha256 = SHA256.Create();
                var hashBytes = await sha256.ComputeHashAsync(stream);
                return Convert.ToHexString(hashBytes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to calculate file hash for: {FileName}", file.FileName);
                return string.Empty;
            }
        }
    }
}