namespace SOAP.Web.Services.Interfaces
{
    public interface IDataProtectionService
    {
        string EncryptPersonalData(string plainText);
        string DecryptPersonalData(string cipherText);
        string EncryptSensitiveField(string value);
        string DecryptSensitiveField(string encryptedValue);
        bool IsEncrypted(string value);
    }
} 