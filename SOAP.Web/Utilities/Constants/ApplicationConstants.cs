namespace SOAP.Web.Utilities.Constants
{
    public static class ApplicationConstants
    {
        public static class Roles
        {
            public const string Parent = "Parent";
            public const string Admin = "Admin";
            public const string SuperAdmin = "SuperAdmin";
        }

        public static class ApplicationStatus
        {
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Incomplete = "Incomplete";
            public const string UnderReview = "Under Review";
        }

        public static class DocumentTypes
        {
            public const string KcpeSlip = "KCPE Slip";
            public const string BirthCertificate = "Birth Certificate";
            public const string MedicalForm = "Medical Form";
            public const string ParentId = "Parent ID";
            public const string PassportPhoto = "Passport Photo";
        }

        public static class DocumentStatus
        {
            public const string Uploaded = "Uploaded";
            public const string Verified = "Verified";
            public const string Rejected = "Rejected";
            public const string Pending = "Pending";
        }

        public static class BoardingStatus
        {
            public const string Day = "Day";
            public const string Boarding = "Boarding";
        }

        public static class SmsMessageTypes
        {
            public const string Incoming = "Incoming";
            public const string Outgoing = "Outgoing";
        }

        public static class SmsStatus
        {
            public const string Sent = "Sent";
            public const string Delivered = "Delivered";
            public const string Failed = "Failed";
            public const string Pending = "Pending";
        }

        public static class FileUpload
        {
            public const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB
            public static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
            public static readonly string[] AllowedMimeTypes = 
            { 
                "application/pdf", 
                "image/jpeg", 
                "image/jpg", 
                "image/png" 
            };
        }

        public static class ValidationMessages
        {
            public const string RequiredField = "This field is required.";
            public const string InvalidPhoneNumber = "Please enter a valid phone number.";
            public const string InvalidKcpeNumber = "Please enter a valid KCPE index number.";
            public const string InvalidFileType = "Please upload a valid file type (PDF, JPG, PNG).";
            public const string FileTooLarge = "File size must be less than 2MB.";
            public const string InvalidAge = "Student age must be between 13 and 20 years.";
        }

        public static class CacheKeys
        {
            public const string Schools = "schools_list";
            public const string ApplicationStats = "application_stats";
            public const string DashboardData = "dashboard_data";
        }

        public static class SessionKeys
        {
            public const string UserId = "UserId";
            public const string UserRole = "UserRole";
            public const string SchoolId = "SchoolId";
            public const string PhoneNumber = "PhoneNumber";
        }
    }
}