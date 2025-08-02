namespace SOAP.Web.Models
{
    /// <summary>
    /// Generic result wrapper for service operations
    /// Demonstrates: Encapsulation, Error Handling Pattern
    /// </summary>
    public class Result<T>
    {
        public bool Success { get; private set; }
        public T? Data { get; private set; }
        public string? ErrorMessage { get; private set; }
        public List<string> Errors { get; private set; } = new();

        protected Result(bool success, T? data, string? errorMessage, List<string>? errors = null)
        {
            Success = success;
            Data = data;
            ErrorMessage = errorMessage;
            Errors = errors ?? new List<string>();
        }

        public static Result<T> SuccessResult(T data)
        {
            return new Result<T>(true, data, null);
        }

        public static Result<T> FailureResult(string errorMessage)
        {
            return new Result<T>(false, default, errorMessage);
        }

        public static Result<T> FailureResult(List<string> errors)
        {
            return new Result<T>(false, default, errors.FirstOrDefault(), errors);
        }
    }

    /// <summary>
    /// Simple result for operations that don't return data
    /// </summary>
    public class Result
    {
        public bool Success { get; private set; }
        public string? ErrorMessage { get; private set; }
        public List<string> Errors { get; private set; } = new();

        protected Result(bool success, string? errorMessage, List<string>? errors = null)
        {
            Success = success;
            ErrorMessage = errorMessage;
            Errors = errors ?? new List<string>();
        }

        public static Result SuccessResult()
        {
            return new Result(true, null);
        }

        public static Result FailureResult(string errorMessage)
        {
            return new Result(false, errorMessage);
        }

        public static Result FailureResult(List<string> errors)
        {
            return new Result(false, errors.FirstOrDefault(), errors);
        }
    }

    /// <summary>
    /// Application-specific result models
    /// </summary>
    public class ApplicationResult : Result<int>
    {
        private ApplicationResult(bool success, int applicationId, string? errorMessage, List<string>? errors = null)
            : base(success, applicationId, errorMessage, errors)
        {
        }

        public static new ApplicationResult Success(int applicationId)
        {
            return new ApplicationResult(true, applicationId, null);
        }

        public static ApplicationResult Failure(string errorMessage)
        {
            return new ApplicationResult(false, 0, errorMessage);
        }

        public static ApplicationResult Failure(List<string> errors)
        {
            return new ApplicationResult(false, 0, errors.FirstOrDefault(), errors);
        }
    }

    /// <summary>
    /// Registration result model
    /// </summary>
    public class RegistrationResult : Result<string>
    {
        private RegistrationResult(bool success, string? userId, string? errorMessage, List<string>? errors = null)
            : base(success, userId, errorMessage, errors)
        {
        }

        public static new RegistrationResult Success(string userId)
        {
            return new RegistrationResult(true, userId, null);
        }

        public static RegistrationResult Failure(string errorMessage)
        {
            return new RegistrationResult(false, null, errorMessage);
        }

        public static RegistrationResult Failure(List<string> errors)
        {
            return new RegistrationResult(false, null, errors.FirstOrDefault(), errors);
        }
    }

    /// <summary>
    /// OTP result model
    /// </summary>
    public class OtpResult : Result
    {
        public string? OtpId { get; private set; }
        public DateTime? ExpiresAt { get; private set; }

        private OtpResult(bool success, string? errorMessage, string? otpId = null, DateTime? expiresAt = null)
            : base(success, errorMessage)
        {
            OtpId = otpId;
            ExpiresAt = expiresAt;
        }

        public static new OtpResult Success(string otpId, DateTime expiresAt)
        {
            return new OtpResult(true, null, otpId, expiresAt);
        }

        public static OtpResult Failure(string errorMessage)
        {
            return new OtpResult(false, errorMessage);
        }
    }

    /// <summary>
    /// Approval result model
    /// </summary>
    public class ApprovalResult : Result
    {
        public string? AdmissionCode { get; private set; }
        public DateTime? ApprovedAt { get; private set; }

        private ApprovalResult(bool success, string? errorMessage, string? admissionCode = null, DateTime? approvedAt = null)
            : base(success, errorMessage)
        {
            AdmissionCode = admissionCode;
            ApprovedAt = approvedAt;
        }

        public static new ApprovalResult Success(string admissionCode, DateTime approvedAt)
        {
            return new ApprovalResult(true, null, admissionCode, approvedAt);
        }

        public static ApprovalResult Failure(string errorMessage)
        {
            return new ApprovalResult(false, errorMessage);
        }
    }

    /// <summary>
    /// Rejection result model
    /// </summary>
    public class RejectionResult : Result
    {
        public string? Reason { get; private set; }
        public DateTime? RejectedAt { get; private set; }

        private RejectionResult(bool success, string? errorMessage, string? reason = null, DateTime? rejectedAt = null)
            : base(success, errorMessage)
        {
            Reason = reason;
            RejectedAt = rejectedAt;
        }

        public static new RejectionResult Success(string reason, DateTime rejectedAt)
        {
            return new RejectionResult(true, null, reason, rejectedAt);
        }

        public static RejectionResult Failure(string errorMessage)
        {
            return new RejectionResult(false, errorMessage);
        }
    }
}