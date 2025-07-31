namespace SOAP.Web.Configuration
{
    public class SecurityConfig
    {
        public AuthenticationConfig Authentication { get; set; } = new();
        public DataProtectionConfig DataProtection { get; set; } = new();
        public RateLimitingConfig RateLimit { get; set; } = new();
        public AuditConfig Audit { get; set; } = new();
        public FileUploadConfig FileUpload { get; set; } = new();
        public EncryptionConfig Encryption { get; set; } = new();
    }

    public class AuthenticationConfig
    {
        public int SessionTimeoutMinutes { get; set; } = 20;
        public int MaxFailedLoginAttempts { get; set; } = 5;
        public int AccountLockoutMinutes { get; set; } = 30;
        public int OtpExpiryMinutes { get; set; } = 5;
        public int OtpLength { get; set; } = 6;
        public int MaxOtpAttempts { get; set; } = 3;
        public bool RequirePhoneVerification { get; set; } = true;
        public bool EnableConcurrentSessionPrevention { get; set; } = true;
    }

    public class DataProtectionConfig
    {
        public string ApplicationName { get; set; } = "SOAP.Web";
        public int KeyLifetimeDays { get; set; } = 90;
        public bool EncryptPersonalData { get; set; } = true;
        public string[] SensitiveFields { get; set; } = 
        {
            "ParentPhone", "HomeAddress", "MedicalConditions", 
            "EmergencyContact", "PhoneNumber"
        };
    }

    public class RateLimitingConfig
    {
        public LoginRateLimit Login { get; set; } = new();
        public OtpRateLimit Otp { get; set; } = new();
        public FileUploadRateLimit FileUpload { get; set; } = new();
        public ApiRateLimit Api { get; set; } = new();
    }

    public class LoginRateLimit
    {
        public int MaxAttempts { get; set; } = 5;
        public int WindowMinutes { get; set; } = 15;
        public int BlockDurationMinutes { get; set; } = 30;
    }

    public class OtpRateLimit
    {
        public int MaxRequests { get; set; } = 3;
        public int WindowMinutes { get; set; } = 5;
        public int BlockDurationMinutes { get; set; } = 15;
    }

    public class FileUploadRateLimit
    {
        public int MaxUploads { get; set; } = 10;
        public int WindowMinutes { get; set; } = 1;
        public long MaxFileSizeBytes { get; set; } = 2 * 1024 * 1024; // 2MB
    }

    public class ApiRateLimit
    {
        public int MaxRequests { get; set; } = 100;
        public int WindowMinutes { get; set; } = 1;
    }

    public class AuditConfig
    {
        public bool EnableSecurityAuditLogging { get; set; } = true;
        public bool LogSuccessfulLogins { get; set; } = true;
        public bool LogFailedLogins { get; set; } = true;
        public bool LogDataAccess { get; set; } = true;
        public bool LogFileUploads { get; set; } = true;
        public bool LogAdminActions { get; set; } = true;
        public int RetentionDays { get; set; } = 2555; // 7 years
        public string[] SensitiveEndpoints { get; set; } = 
        {
            "/Account/Login", "/Account/Register", "/Parent/Application/Submit",
            "/Admin/Application/Review", "/Document/Upload", "/Document/View"
        };
    }

    public class FileUploadConfig
    {
        public string[] AllowedExtensions { get; set; } = { ".pdf", ".jpg", ".jpeg", ".png" };
        public string[] AllowedMimeTypes { get; set; } = 
        {
            "application/pdf", "image/jpeg", "image/png"
        };
        public long MaxFileSizeBytes { get; set; } = 2 * 1024 * 1024; // 2MB
        public bool EnableVirusScanning { get; set; } = false; // Set to true in production
        public bool ValidateFileSignatures { get; set; } = true;
        public string SecureStoragePath { get; set; } = "wwwroot/secure-uploads";
        public bool EncryptStoredFiles { get; set; } = false; // Set to true for highly sensitive docs
    }

    public class EncryptionConfig
    {
        public string Algorithm { get; set; } = "AES-256-GCM";
        public bool EncryptDatabaseConnections { get; set; } = true;
        public bool EncryptCookies { get; set; } = true;
        public bool EncryptViewState { get; set; } = true;
        public string KeyDerivationAlgorithm { get; set; } = "PBKDF2";
        public int KeyDerivationIterations { get; set; } = 100000;
    }
}