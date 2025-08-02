using SOAP.Web.Models.Entities;

namespace SOAP.Web.Services.Interfaces
{
    /// <summary>
    /// Service for validating user roles and permissions with security checks
    /// </summary>
    public interface IRoleValidationService
    {
        /// <summary>
        /// Validates if a user can have the specified role
        /// Includes security checks for Platform Admin
        /// </summary>
        Task<bool> CanUserHaveRoleAsync(string phoneNumber, string role, int? schoolId = null);

        /// <summary>
        /// Validates if a user can access a specific school's data
        /// </summary>
        Task<bool> CanAccessSchoolDataAsync(User user, int schoolId);

        /// <summary>
        /// Validates if a user can access a specific application
        /// </summary>
        Task<bool> CanAccessApplicationAsync(User user, int applicationId);

        /// <summary>
        /// Gets the appropriate dashboard route for a user's role
        /// </summary>
        string GetDashboardRoute(string role);

        /// <summary>
        /// Validates role assignment and logs security violations
        /// </summary>
        Task<bool> ValidateRoleAssignmentAsync(User user, string newRole);

        /// <summary>
        /// Checks if user is attempting privilege escalation
        /// </summary>
        Task<bool> IsPrivilegeEscalationAttemptAsync(User user, string requestedRole);
    }
}