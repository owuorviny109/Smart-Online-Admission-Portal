namespace SOAP.Web.Services.Interfaces
{
    /// <summary>
    /// Multi-Factor Authentication service interface
    /// </summary>
    public interface IMultiFactorAuthService
    {
        /// <summary>
        /// Initiates MFA for Platform Admin with multiple contact options
        /// </summary>
        Task<MfaInitiationResult> InitiatePlatformAdminMfaAsync(string phoneNumber, string email, string ipAddress);

        /// <summary>
        /// Verifies OTP with advanced security checks
        /// </summary>
        Task<MfaVerificationResult> VerifyOtpAsync(string sessionId, string otpCode, string ipAddress);

        /// <summary>
        /// Provides backup access options if primary method fails
        /// </summary>
        Task<BackupAccessResult> InitiateBackupAccessAsync(string primaryPhone, string backupContactMethod);

        /// <summary>
        /// Cleanup expired OTP sessions
        /// </summary>
        void CleanupExpiredSessions();
    }
}