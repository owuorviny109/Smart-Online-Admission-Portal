using System.ComponentModel.DataAnnotations;
using SOAP.Web.Utilities.Constants;

namespace SOAP.Web.Models.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = "";

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = UserRoles.Parent; // Default to Parent for security

        public int? SchoolId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Security properties
        public DateTimeOffset? LastLoginAt { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;

        public DateTimeOffset? LockedUntil { get; set; }

        // GDPR compliance properties
        public DateTimeOffset? DeletionDate { get; set; }

        [StringLength(200)]
        public string? DeletionReason { get; set; }

        // Navigation properties
        public virtual School? School { get; set; }

        /// <summary>
        /// SECURITY: Validates if this user can have Platform Admin role
        /// Only the system owner's phone number can be Platform Admin
        /// </summary>
        public bool CanBePlatformAdmin()
        {
            return UserRoles.CanBePlatformAdmin(PhoneNumber);
        }

        /// <summary>
        /// SECURITY: Validates if role assignment is legitimate
        /// </summary>
        public bool IsValidRoleAssignment()
        {
            // Check if role is valid
            if (!UserRoles.IsValidRole(Role))
                return false;

            // Platform Admin can only be assigned to system owner
            if (Role == UserRoles.PlatformAdmin)
                return CanBePlatformAdmin();

            // School Admin must have a school
            if (Role == UserRoles.SchoolAdmin)
                return SchoolId.HasValue;

            return true;
        }

        /// <summary>
        /// Gets user-friendly role display name
        /// </summary>
        public string GetRoleDisplayName()
        {
            return UserRoles.GetRoleDisplayName(Role);
        }

        /// <summary>
        /// Checks if user can access admin areas
        /// </summary>
        public bool CanAccessAdmin()
        {
            return UserRoles.CanAccessAdmin(Role);
        }
    }
}