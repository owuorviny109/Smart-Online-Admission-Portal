namespace SOAP.Web.Configuration
{
    public class FileStorageConfig
    {
        public string StorageType { get; set; } = "Local"; // Local, Azure, AWS
        public string BasePath { get; set; } = "wwwroot/uploads";
        public long MaxFileSize { get; set; } = 2 * 1024 * 1024; // 2MB
        public string[] AllowedExtensions { get; set; } = { ".pdf", ".jpg", ".jpeg", ".png" };
        public string[] AllowedMimeTypes { get; set; } = { "application/pdf", "image/jpeg", "image/jpg", "image/png" };
        public bool EnableVirusScanning { get; set; } = false;
        public int CleanupIntervalHours { get; set; } = 24;
        public int TempFileRetentionHours { get; set; } = 2;
        
        // Azure Blob Storage settings
        public class AzureBlobConfig
        {
            public string ConnectionString { get; set; } = string.Empty;
            public string ContainerName { get; set; } = "documents";
            public bool EnableCdn { get; set; } = false;
            public string CdnEndpoint { get; set; } = string.Empty;
        }
        
        // AWS S3 settings
        public class AwsS3Config
        {
            public string AccessKey { get; set; } = string.Empty;
            public string SecretKey { get; set; } = string.Empty;
            public string BucketName { get; set; } = "soap-documents";
            public string Region { get; set; } = "us-east-1";
            public bool EnableCloudFront { get; set; } = false;
            public string CloudFrontDomain { get; set; } = string.Empty;
        }
        
        public AzureBlobConfig AzureBlob { get; set; } = new AzureBlobConfig();
        public AwsS3Config AwsS3 { get; set; } = new AwsS3Config();
    }
}