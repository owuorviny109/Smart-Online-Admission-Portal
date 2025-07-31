using SOAP.Web.Services.Interfaces;
using SOAP.Web.Models;
using System.Security.Cryptography;

namespace SOAP.Web.Services.Validators
{
    /// <summary>
    /// Image document validator implementation
    /// Demonstrates: Strategy Pattern, SRP, Encapsulation
    /// </summary>
    public class ImageDocumentValidator : IDocumentValidator
    {
        private readonly ILogger<ImageDocumentValidator> _logger;
        private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB
        
        // Image file signatures
        private static readonly Dictionary<string, byte[]> ImageSignatures = new()
        {
            { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
            { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
            { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } }
        };

        public IEnumerable<DocumentType> SupportedTypes => new[]
        {
            DocumentType.PassportPhoto,
            DocumentType.ParentId,
            DocumentType.Other
        };

        public int Priority => 2;

        public ImageDocumentValidator(ILogger<ImageDocumentValidator> logger)
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

                // Image-specific validation
                var imageValidation = await ValidateImagePropertiesAsync(file);
                if (!imageValidation.IsValid)
                    return imageValidation;

                // Extract metadata
                var metadata = await ExtractMetadataAsync(file);
                var fileHash = await CalculateFileHashAsync(file);

                _logger.LogInformation("Image validation successful for file: {FileName}", file.FileName);

                return DocumentValidationResult.Success(metadata, fileHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image validation failed for file: {FileName}", file.FileName);
                return DocumentValidationResult.Failure($"Image validation error: {ex.Message}");
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
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!ImageSignatures.ContainsKey(extension))
                {
                    errors.Add("File must be a JPEG or PNG image");
                }
            }

            // Check content type
            if (file != null)
            {
                var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
                if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                {
                    errors.Add("File content type must be image/jpeg or image/png");
                }
            }

            return errors.Any() 
                ? DocumentValidationResult.Failure(errors.ToArray())
                : DocumentValidationResult.Success();
        }

        /// <summary>
        /// Validates image file signature (magic bytes)
        /// Encapsulation: Private method for signature validation
        /// </summary>
        private async Task<DocumentValidationResult> ValidateFileSignatureAsync(IFormFile file)
        {
            try
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!ImageSignatures.TryGetValue(extension, out var expectedSignature))
                {
                    return DocumentValidationResult.Failure("Unsupported image format");
                }

                using var stream = file.OpenReadStream();
                var buffer = new byte[expectedSignature.Length];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead < expectedSignature.Length)
                {
                    return DocumentValidationResult.Failure("File is too small to be a valid image");
                }

                if (!buffer.Take(expectedSignature.Length).SequenceEqual(expectedSignature))
                {
                    return DocumentValidationResult.Failure("File does not have a valid image signature");
                }

                return DocumentValidationResult.Success();
            }
            catch (Exception ex)
            {
                return DocumentValidationResult.Failure($"Failed to validate image signature: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates image-specific properties
        /// Encapsulation: Private method for image validation
        /// </summary>
        private async Task<DocumentValidationResult> ValidateImagePropertiesAsync(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                
                // Basic image validation - check if we can read basic properties
                // In a real implementation, you would use a proper image library like ImageSharp
                
                // For now, just validate that the file can be opened as a stream
                if (!stream.CanRead)
                {
                    return DocumentValidationResult.Failure("Image file cannot be read");
                }

                // Check minimum file size (very small files are likely corrupted)
                if (stream.Length < 1024) // 1KB minimum
                {
                    return DocumentValidationResult.Failure("Image file is too small to be valid");
                }

                return DocumentValidationResult.Success();
            }
            catch (Exception ex)
            {
                return DocumentValidationResult.Failure($"Failed to validate image properties: {ex.Message}");
            }
        }

        /// <summary>
        /// Extracts metadata from image file
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

                // In a real implementation, you would extract EXIF data, dimensions, etc.
                // For now, just return basic metadata
                
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract image metadata for file: {FileName}", file.FileName);
                
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