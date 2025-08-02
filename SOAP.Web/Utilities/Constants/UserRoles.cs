using System.ComponentModel;

namespace SOAP.Web.Utilities.Constants
{
    /// <summary>
    /// Defines the three user roles in the SOAP system with strict security validation
    /// </summary>
    public static class UserRoles
    {
        /// <summary>
        /// Platform Administrator - System owner with platform-level control
        /// Only YOU should have this role - hardcoded security check required
        /// </summary>
        public const string PlatformAdmin = "PlatformAdmin";
        
        /// <summary>
        /// School Administrator - Complete control over their school's data only
        /// One per school, cannot access other schools' data
        /// </summary>
        public const string SchoolAdmin = "SchoolAdmin";
        
        /// <summary>
        /// Parent - Personal application data access only
        /// Cannot access any administrative functions
        /// </summary>
        public const string Parent = "Parent";

        /// <summary>
        /// All valid roles for validation
        /// </summary>
        public static readonly string[] AllRoles = { PlatformAdmin, SchoolAdmin, Parent };

        /// <summary>
        /// Administrative roles that can access admin areas
        /// </summary>
        public static readonly string[] AdminRoles = { PlatformAdmin, SchoolAdmin };

        /// <summary>
        /// SECURITY: Your authorized phone numbers for Platform Admin verification
        /// Add your primary and backup phone numbers here
        /// </summary>
        public static readonly string[] PLATFORM_ADMIN_PHONES = {
            "+254796915745",  // Your primary phone number
            "+254739351850"   // Add your backup phone number here
        };

        /// <summary>
        /// SECURITY: Your authorized emails for Platform Admin verification
        /// Add your primary and backup emails here
        /// </summary>
        public static readonly string[] PLATFORM_ADMIN_EMAILS = {
            "owuorvincent069@gmail.com",  // Your primary email
            "owuorvincent958@gmail.com"          // Add your backup email here
        };

        /// <summary>
        /// LEGACY: Keep for backward compatibility
        /// </summary>
        public const string PLATFORM_ADMIN_PHONE = "+254796915745";
        public const string PLATFORM_ADMIN_EMAIL = "owuorvincent069@gmail.com";

        /// <summary>
        /// Validates if a role is valid
        /// </summary>
        public static bool IsValidRole(string role)
        {
            return AllRoles.Contains(role);
        }

        /// <summary>
        /// Checks if role can access admin areas
        /// </summary>
        public static bool CanAccessAdmin(string role)
        {
            return AdminRoles.Contains(role);
        }

        /// <summary>
        /// SECURITY: Validates if user can be Platform Admin
        /// Checks against ALL your authorized phone numbers and emails
        /// </summary>
        public static bool CanBePlatformAdmin(string phoneNumber, string email = null)
        {
            var phoneMatch = PLATFORM_ADMIN_PHONES.Contains(phoneNumber);
            var emailMatch = string.IsNullOrEmpty(email) || PLATFORM_ADMIN_EMAILS.Contains(email);
            return phoneMatch && emailMatch;
        }

        /// <summary>
        /// SECURITY: Gets the contact method used for Platform Admin verification
        /// Returns which phone/email combination was used
        /// </summary>
        public static (bool isValid, string phoneUsed, string emailUsed) ValidatePlatformAdminCredentials(string phoneNumber, string email)
        {
            var phoneMatch = PLATFORM_ADMIN_PHONES.Contains(phoneNumber);
            var emailMatch = PLATFORM_ADMIN_EMAILS.Contains(email);
            
            if (phoneMatch && emailMatch)
            {
                return (true, phoneNumber, email);
            }
            
            return (false, string.Empty, string.Empty);
        }

        /// <summary>
        /// SECURITY: Gets all authorized contact methods for Platform Admin
        /// Used for MFA and account recovery
        /// </summary>
        public static (string[] phones, string[] emails) GetPlatformAdminContacts()
        {
            return (PLATFORM_ADMIN_PHONES, PLATFORM_ADMIN_EMAILS);
        }

        /// <summary>
        /// Gets user-friendly role name
        /// </summary>
        public static string GetRoleDisplayName(string role)
        {
            return role switch
            {
                PlatformAdmin => "Platform Administrator",
                SchoolAdmin => "School Administrator", 
                Parent => "Parent",
                _ => "Unknown Role"
            };
        }
    }
}