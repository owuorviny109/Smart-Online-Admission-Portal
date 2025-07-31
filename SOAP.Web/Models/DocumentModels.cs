namespace SOAP.Web.Models
{
    /// <summary>
    /// Enumeration of document types
    /// </summary>
    public enum DocumentType
    {
        BirthCertificate,
        KcpeCertificate,
        ParentId,
        PassportPhoto,
        MedicalForm,
        TransferLetter,
        Other
    }

    /// <summary>
    /// Result of document validation
    /// Encapsulation: Contains validation result and details
    /// </summary>
    public class DocumentValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public DocumentMetadata? Metadata { get; set; }
        public string? FileHash { get; set; }

        public static DocumentValidationResult Success(DocumentMetadata? metadata = null, string? fileHash = null)
        {
            return new DocumentValidationResult
            {
                IsValid = true,
                Metadata = metadata,
                FileHash = fileHash
            };
        }

        public static DocumentValidationResult Failure(params string[] errors)
        {
            return new DocumentValidationResult
            {
                IsValid = false,
                Errors = errors.ToList()
            };
        }

        public static DocumentValidationResult Warning(string warning, DocumentMetadata? metadata = null)
        {
            return new DocumentValidationResult
            {
                IsValid = true,
                Warnings = new List<string> { warning },
                Metadata = metadata
            };
        }
    }

    /// <summary>
    /// Document metadata extracted during validation
    /// Encapsulation: Contains document properties
    /// </summary>
    public class DocumentMetadata
    {
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }
        public string ContentType { get; set; } = "";
        public string FileExtension { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public int? PageCount { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public Dictionary<string, object> CustomProperties { get; set; } = new();
    }

    /// <summary>
    /// Document upload result
    /// </summary>
    public class DocumentUploadResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? DocumentId { get; set; }
        public string? SecureFileName { get; set; }
        public string? FilePath { get; set; }
        public long FileSize { get; set; }
        public string? FileHash { get; set; }

        public static DocumentUploadResult SuccessResult(string documentId, string secureFileName, string filePath, long fileSize, string? fileHash = null)
        {
            return new DocumentUploadResult
            {
                Success = true,
                DocumentId = documentId,
                SecureFileName = secureFileName,
                FilePath = filePath,
                FileSize = fileSize,
                FileHash = fileHash
            };
        }

        public static DocumentUploadResult FailureResult(string errorMessage)
        {
            return new DocumentUploadResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }

    /// <summary>
    /// Storage result for file operations
    /// </summary>
    public class StorageResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Path { get; set; }
        public string? Url { get; set; }
        public long FileSize { get; set; }

        public static StorageResult SuccessResult(string path, long fileSize, string? url = null)
        {
            return new StorageResult
            {
                Success = true,
                Path = path,
                FileSize = fileSize,
                Url = url
            };
        }

        public static StorageResult FailureResult(string errorMessage)
        {
            return new StorageResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}