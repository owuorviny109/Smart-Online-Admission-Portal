using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;
using SOAP.Web.Models.Entities;
using SOAP.Web.Services.Interfaces;
using SOAP.Web.Utilities.Constants;

namespace SOAP.Web.Services
{
    /// <summary>
    /// Secure role validation service with anti-hacking measures
    /// </summary>
    public class RoleValidationService : IRoleValidationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoleValidationService> _logger;

        public RoleValidationService(ApplicationDbContext context, ILogger<RoleValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// SECURITY: Validates if a user can have the specified role
        /// Platform Admin role is ONLY for the system owner
        /// </summary>
        public async Task<bool> CanUserHaveRoleAsync(string phoneNumber, string role, int? schoolId = null)
        {
            // Validate role exists
            if (!UserRoles.IsValidRole(role))
            {
                _logger.LogWarning("Invalid role assignment attempted: {Role} for phone: {Phone}", role, phoneNumber);
                return false;
            }

            // SECURITY: Platform Admin can ONLY be assigned to YOUR phone number
            if (role == UserRoles.PlatformAdmin)
            {
                var canBePlatformAdmin = UserRoles.CanBePlatformAdmin(phoneNumber);
                if (!canBePlatformAdmin)
                {
                    _logger.LogCritical("SECURITY VIOLATION: Unauthorized Platform Admin assignment attempted for phone: {Phone}", phoneNumber);
                    // TODO: Send security alert email/SMS to you
                }
                return canBePlatformAdmin;
            }

            // School Admin must have a valid school
            if (role == UserRoles.SchoolAdmin)
            {
                if (!schoolId.HasValue)
                {
                    _logger.LogWarning("School Admin role requires SchoolId for phone: {Phone}", phoneNumber);
                    return false;
                }

                // Verify school exists and is active
                var schoolExists = await _context.Schools
                    .AnyAsync(s => s.Id == schoolId.Value && s.IsActive);
                
                if (!schoolExists)
                {
                    _logger.LogWarning("Invalid school assignment for School Admin: {SchoolId}, Phone: {Phone}", schoolId, phoneNumber);
                    return false;
                }
            }

            // Parent role is always allowed (they register themselves)
            return true;
        }

        /// <summary>
        /// SECURITY: Validates if user can access specific school data
        /// </summary>
        public async Task<bool> CanAccessSchoolDataAsync(User user, int schoolId)
        {
            // Platform Admin can access all schools (for system management)
            if (user.Role == UserRoles.PlatformAdmin)
            {
                // Double-check Platform Admin legitimacy
                if (!UserRoles.CanBePlatformAdmin(user.PhoneNumber))
                {
                    _logger.LogCritical("SECURITY VIOLATION: Fake Platform Admin detected: {UserId}, Phone: {Phone}", user.Id, user.PhoneNumber);
                    return false;
                }
                return true;
            }

            // School Admin can only access their own school
            if (user.Role == UserRoles.SchoolAdmin)
            {
                var hasAccess = user.SchoolId == schoolId;
                if (!hasAccess)
                {
                    _logger.LogWarning("SECURITY: School Admin {UserId} attempted to access school {SchoolId} (their school: {UserSchoolId})", 
                        user.Id, schoolId, user.SchoolId);
                }
                return hasAccess;
            }

            // Parents cannot access school data directly
            _logger.LogWarning("SECURITY: Parent {UserId} attempted to access school data for school {SchoolId}", user.Id, schoolId);
            return false;
        }

        /// <summary>
        /// SECURITY: Validates if user can access specific application
        /// </summary>
        public async Task<bool> CanAccessApplicationAsync(User user, int applicationId)
        {
            var application = await _context.Applications
                .Include(a => a.School)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
            {
                _logger.LogWarning("Application not found: {ApplicationId} requested by user {UserId}", applicationId, user.Id);
                return false;
            }

            // Platform Admin can access all applications
            if (user.Role == UserRoles.PlatformAdmin)
            {
                if (!UserRoles.CanBePlatformAdmin(user.PhoneNumber))
                {
                    _logger.LogCritical("SECURITY VIOLATION: Fake Platform Admin accessing application: {UserId}", user.Id);
                    return false;
                }
                return true;
            }

            // School Admin can access applications to their school
            if (user.Role == UserRoles.SchoolAdmin)
            {
                var hasAccess = user.SchoolId == application.SchoolId;
                if (!hasAccess)
                {
                    _logger.LogWarning("SECURITY: School Admin {UserId} attempted to access application {ApplicationId} from different school", 
                        user.Id, applicationId);
                }
                return hasAccess;
            }

            // Parent can only access their own applications
            if (user.Role == UserRoles.Parent)
            {
                var hasAccess = application.ParentPhone == user.PhoneNumber;
                if (!hasAccess)
                {
                    _logger.LogWarning("SECURITY: Parent {UserId} attempted to access application {ApplicationId} belonging to different parent", 
                        user.Id, applicationId);
                }
                return hasAccess;
            }

            return false;
        }

        /// <summary>
        /// Gets the appropriate dashboard route based on role
        /// </summary>
        public string GetDashboardRoute(string role)
        {
            return role switch
            {
                UserRoles.PlatformAdmin => "/Dashboard",
                UserRoles.SchoolAdmin => "/Admin/Dashboard",
                UserRoles.Parent => "/Parent/Home",
                _ => "/Account/Login"
            };
        }

        /// <summary>
        /// SECURITY: Validates role assignment and prevents privilege escalation
        /// </summary>
        public async Task<bool> ValidateRoleAssignmentAsync(User user, string newRole)
        {
            // Check if role change is valid
            if (!UserRoles.IsValidRole(newRole))
            {
                _logger.LogWarning("Invalid role assignment attempted: {NewRole} for user {UserId}", newRole, user.Id);
                return false;
            }

            // CRITICAL: Only YOU can assign Platform Admin role
            if (newRole == UserRoles.PlatformAdmin)
            {
                var canAssign = UserRoles.CanBePlatformAdmin(user.PhoneNumber);
                if (!canAssign)
                {
                    _logger.LogCritical("SECURITY VIOLATION: Unauthorized Platform Admin assignment for user {UserId}, Phone: {Phone}", 
                        user.Id, user.PhoneNumber);
                    // TODO: Send immediate security alert
                }
                return canAssign;
            }

            // Log all role changes for audit
            _logger.LogInformation("Role change: User {UserId} from {OldRole} to {NewRole}", user.Id, user.Role, newRole);
            return true;
        }

        /// <summary>
        /// SECURITY: Detects privilege escalation attempts
        /// </summary>
        public async Task<bool> IsPrivilegeEscalationAttemptAsync(User user, string requestedRole)
        {
            // Any attempt to become Platform Admin (except by you) is escalation
            if (requestedRole == UserRoles.PlatformAdmin && user.Role != UserRoles.PlatformAdmin)
            {
                if (!UserRoles.CanBePlatformAdmin(user.PhoneNumber))
                {
                    _logger.LogCritical("PRIVILEGE ESCALATION ATTEMPT: User {UserId} ({Phone}) attempting to become Platform Admin", 
                        user.Id, user.PhoneNumber);
                    return true;
                }
            }

            // Parent trying to become admin
            if (user.Role == UserRoles.Parent && requestedRole == UserRoles.SchoolAdmin)
            {
                _logger.LogWarning("PRIVILEGE ESCALATION ATTEMPT: Parent {UserId} attempting to become School Admin", user.Id);
                return true;
            }

            return false;
        }
    }
}