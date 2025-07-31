using Microsoft.AspNetCore.DataProtection;
using SOAP.Web.Services.Interfaces;

namespace SOAP.Web.Services
{
    public class DataProtectionService : IDataProtectionService
    {
        private readonly IDataProtector _protector;
        private readonly ILogger<DataProtectionService> _logger;

        public DataProtectionService(IDataProtectionProvider provider, ILogger<DataProtectionService> logger)
        {
            _protector = provider.CreateProtector("SOAP.PersonalData.v1");
            _logger = logger;
        }

        public string EncryptPersonalData(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                return _protector.Protect(plainText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to encrypt personal data");
                return plainText; // Return original if encryption fails
            }
        }

        public string DecryptPersonalData(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            try
            {
                return _protector.Unprotect(cipherText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt personal data");
                return cipherText; // Return original if decryption fails
            }
        }

        public string EncryptSensitiveField(string value)
        {
            return EncryptPersonalData(value);
        }

        public string DecryptSensitiveField(string encryptedValue)
        {
            return DecryptPersonalData(encryptedValue);
        }

        public bool IsEncrypted(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            try
            {
                // Try to decrypt - if it succeeds, it was encrypted
                _protector.Unprotect(value);
                return true;
            }
            catch
            {
                // If decryption fails, it was not encrypted
                return false;
            }
        }
    }
} 