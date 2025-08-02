using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;
using SOAP.Web.Models.Entities;
using SOAP.Web.Services.Interfaces;
using SOAP.Web.Utilities.Constants;

namespace SOAP.Web.Services
{
    /// <summary>
    /// Secure data filtering service that prevents unauthorized data access
    /// </summary>
    public class DataFilterService : IDataFilterService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataFilterService> _logger;
        private readonly IRoleValidationService _roleValidationService;

        public DataFilterService(
            ApplicationDbContext context,
            ILogger<DataFilterService> logger,
            IRoleValidationService roleValidationService)
        {
            _context = context;
            _logger = logger;
            _roleValidationService = roleValidationService;
        }

        /// <summary>
        /// SECURITY: Master filter method that routes to specific filters based on entity type
        /// </summary>
        public IQueryable<T> ApplyUserFilter<T>(IQueryable<T> query, User user) where T : class
        {
            // Validate user first
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Attempted data access with null or inactive user");
                return query.Where(_ => false); // Return empty result
            }

            // Route to specific filter based on entity type
            return typeof(T).Name switch
            {
                nameof(Application) => (IQueryable<T>)FilterApplications((IQueryable<Application>)(object)query, user),
                nameof(Document) => (IQueryable<T>)FilterDocuments((IQueryable<Document>)(object)query, user),
                nameof(School) => (IQueryable<T>)FilterSchools((IQueryable<School>)(object)query, user),
                nameof(User) => (IQueryable<T>)FilterUsers((IQueryable<User>)(object)query, user),
                nameof(SmsLog) => (IQueryable<T>)FilterSmsLogs((IQueryable<SmsLog>)(object)query, user),
                nameof(SecurityAuditLog) => (IQueryable<T>)FilterSecurityLogs((IQueryable<SecurityAuditLog>)(object)query, user),
                _ => ApplyGenericFilter(query, user)
            };
        }

        /// <summary>
        /// SECURITY: Filters applications with strict role-based access
        /// </summary>
        public IQueryable<Application> FilterApplications(IQueryable<Application> query, User user)
        {
            return user.Role switch
            {
                UserRoles.PlatformAdmin => ValidatePlatformAdminAndFilter(query, user),
                UserRoles.SchoolAdmin => FilterSchoolAdminApplications(query, user),
                UserRoles.Parent => FilterParentApplications(query, user),
                _ => LogUnauthorizedAccessAndReturnEmpty(query, user, "Applications")
            };
        }

        /// <summary>
        /// SECURITY: Filters documents with strict access control
        /// </summary>
        public IQueryable<Document> FilterDocuments(IQueryable<Document> query, User user)
        {
            return user.Role switch
            {
                UserRoles.PlatformAdmin => ValidatePlatformAdminAndFilter(query, user),
                UserRoles.SchoolAdmin => FilterSchoolAdminDocuments(query, user),
                UserRoles.Parent => FilterParentDocuments(query, user),
                _ => LogUnauthorizedAccessAndReturnEmpty(query, user, "Documents")
            };
        }

        /// <summary>
        /// SECURITY: Filters schools - Parents have NO access to school data
        /// </summary>
        public IQueryable<School> FilterSchools(IQueryable<School> query, User user)
        {
            return user.Role switch
            {
                UserRoles.PlatformAdmin => ValidatePlatformAdminAndFilter(query, user),
                UserRoles.SchoolAdmin => FilterSchoolAdminSchools(query, user),
                UserRoles.Parent => LogUnauthorizedAccessAndReturnEmpty(query, user, "Schools"), // Parents cannot access school data
                _ => LogUnauthorizedAccessAndReturnEmpty(query, user, "Schools")
            };
        }

        /// <summary>
        /// SECURITY: Filters users with strict privacy controls
        /// </summary>
        public IQueryable<User> FilterUsers(IQueryable<User> query, User currentUser)
        {
            return currentUser.Role switch
            {
                UserRoles.PlatformAdmin => ValidatePlatformAdminAndFilter(query, currentUser),
                UserRoles.SchoolAdmin => FilterSchoolAdminUsers(query, currentUser),
                UserRoles.Parent => FilterParentUsers(query, currentUser),
                _ => LogUnauthorizedAccessAndReturnEmpty(query, currentUser, "Users")
            };
        }

        /// <summary>
        /// SECURITY: Filters SMS logs to prevent communication privacy violations
        /// </summary>
        public IQueryable<SmsLog> FilterSmsLogs(IQueryable<SmsLog> query, User user)
        {
            return user.Role switch
            {
                UserRoles.PlatformAdmin => ValidatePlatformAdminAndFilter(query, user),
                UserRoles.SchoolAdmin => FilterSchoolAdminSmsLogs(query, user),
                UserRoles.Parent => FilterParentSmsLogs(query, user),
                _ => LogUnauthorizedAccessAndReturnEmpty(query, user, "SMS Logs")
            };
        }

        /// <summary>
        /// SECURITY: Filters security logs - highly sensitive data
        /// </summary>
        public IQueryable<SecurityAuditLog> FilterSecurityLogs(IQueryable<SecurityAuditLog> query, User user)
        {
            return user.Role switch
            {
                UserRoles.PlatformAdmin => ValidatePlatformAdminAndFilter(query, user),
                UserRoles.SchoolAdmin => FilterSchoolAdminSecurityLogs(query, user),
                UserRoles.Parent => FilterParentSecurityLogs(query, user),
                _ => LogUnauthorizedAccessAndReturnEmpty(query, user, "Security Logs")
            };
        }

        /// <summary>
        /// SECURITY: Validates entity access by ID with comprehensive checks
        /// </summary>
        public async Task<bool> CanAccessEntityAsync<T>(User user, int entityId) where T : class
        {
            try
            {
                var entityType = typeof(T).Name;
                
                // Log access attempt
                _logger.LogInformation("Entity access check: User {UserId} ({Role}) accessing {EntityType} ID {EntityId}", 
                    user.Id, user.Role, entityType, entityId);

                return entityType switch
                {
                    nameof(Application) => await CanAccessApplicationAsync(user, entityId),
                    nameof(Document) => await CanAccessDocumentAsync(user, entityId),
                    nameof(School) => await CanAccessSchoolAsync(user, entityId),
                    nameof(User) => await CanAccessUserAsync(user, entityId),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking entity access for user {UserId}", user.Id);
                return false;
            }
        }

        /// <summary>
        /// SECURITY: Gets comprehensive data scope for user
        /// </summary>
        public DataScope GetUserDataScope(User user)
        {
            var scope = new DataScope();

            switch (user.Role)
            {
                case UserRoles.PlatformAdmin:
                    if (UserRoles.CanBePlatformAdmin(user.PhoneNumber))
                    {
                        scope.CanAccessAllSchools = true;
                        scope.CanAccessAllUsers = true;
                        scope.CanAccessSystemLogs = true;
                        scope.CanAccessBillingData = true;
                        scope.AllowedOperations.AddRange(new[] { "CREATE", "READ", "UPDATE", "DELETE", "EXPORT", "MANAGE" });
                    }
                    else
                    {
                        _logger.LogCritical("SECURITY VIOLATION: Fake Platform Admin detected: {UserId}", user.Id);
                        // Return empty scope for fake Platform Admin
                    }
                    break;

                case UserRoles.SchoolAdmin:
                    if (user.SchoolId.HasValue)
                    {
                        scope.RestrictedToSchoolId = user.SchoolId.Value;
                        scope.AllowedOperations.AddRange(new[] { "CREATE", "READ", "UPDATE", "EXPORT" });
                    }
                    break;

                case UserRoles.Parent:
                    scope.RestrictedToPhoneNumber = user.PhoneNumber;
                    scope.AllowedOperations.AddRange(new[] { "READ", "UPDATE" });
                    break;
            }

            return scope;
        }

        /// <summary>
        /// SECURITY: Validates and logs all data access attempts
        /// </summary>
        public async Task<bool> ValidateAndLogDataAccessAsync<T>(User user, string operation, int? entityId = null) where T : class
        {
            var entityType = typeof(T).Name;
            var scope = GetUserDataScope(user);

            // Check if operation is allowed
            if (!scope.AllowedOperations.Contains(operation))
            {
                _logger.LogWarning("SECURITY: Unauthorized operation {Operation} attempted by user {UserId} on {EntityType}", 
                    operation, user.Id, entityType);
                return false;
            }

            // Log the access attempt
            var securityLog = new SecurityAuditLog
            {
                EventType = $"DATA_ACCESS_{operation}",
                Details = $"User {user.Id} ({user.Role}) accessed {entityType}" + (entityId.HasValue ? $" ID {entityId}" : ""),
                UserId = user.Id.ToString(),
                Timestamp = DateTimeOffset.UtcNow,
                Success = true
            };

            _context.SecurityAuditLogs.Add(securityLog);
            await _context.SaveChangesAsync();

            return true;
        }

        // Private helper methods for specific role filtering

        private IQueryable<T> ValidatePlatformAdminAndFilter<T>(IQueryable<T> query, User user) where T : class
        {
            // CRITICAL: Validate Platform Admin legitimacy
            if (!UserRoles.CanBePlatformAdmin(user.PhoneNumber))
            {
                _logger.LogCritical("SECURITY VIOLATION: Fake Platform Admin attempting data access: {UserId}", user.Id);
                return query.Where(_ => false); // Return empty for fake admin
            }

            _logger.LogInformation("Platform Admin data access: User {UserId}", user.Id);
            return query; // Platform Admin can access all data
        }

        private IQueryable<Application> FilterSchoolAdminApplications(IQueryable<Application> query, User user)
        {
            if (!user.SchoolId.HasValue)
            {
                _logger.LogWarning("School Admin {UserId} has no assigned school", user.Id);
                return query.Where(_ => false);
            }

            _logger.LogDebug("Filtering applications for School Admin {UserId}, School {SchoolId}", user.Id, user.SchoolId);
            return query.Where(a => a.SchoolId == user.SchoolId.Value);
        }

        private IQueryable<Application> FilterParentApplications(IQueryable<Application> query, User user)
        {
            _logger.LogDebug("Filtering applications for Parent {UserId}, Phone {Phone}", user.Id, user.PhoneNumber);
            return query.Where(a => a.ParentPhone == user.PhoneNumber);
        }

        private IQueryable<Document> FilterSchoolAdminDocuments(IQueryable<Document> query, User user)
        {
            if (!user.SchoolId.HasValue)
            {
                return query.Where(_ => false);
            }

            // School Admin can see documents for applications to their school
            return query.Where(d => d.Application.SchoolId == user.SchoolId.Value);
        }

        private IQueryable<Document> FilterParentDocuments(IQueryable<Document> query, User user)
        {
            // Parent can only see their own documents
            return query.Where(d => d.Application.ParentPhone == user.PhoneNumber);
        }

        private IQueryable<School> FilterSchoolAdminSchools(IQueryable<School> query, User user)
        {
            if (!user.SchoolId.HasValue)
            {
                return query.Where(_ => false);
            }

            // School Admin can only see their own school
            return query.Where(s => s.Id == user.SchoolId.Value);
        }

        private IQueryable<User> FilterSchoolAdminUsers(IQueryable<User> query, User currentUser)
        {
            if (!currentUser.SchoolId.HasValue)
            {
                return query.Where(_ => false);
            }

            // School Admin can see users related to their school (other school admins, parents with applications)
            return query.Where(u => 
                (u.Role == UserRoles.SchoolAdmin && u.SchoolId == currentUser.SchoolId.Value) ||
                (u.Role == UserRoles.Parent && _context.Applications.Any(a => a.ParentPhone == u.PhoneNumber && a.SchoolId == currentUser.SchoolId.Value))
            );
        }

        private IQueryable<User> FilterParentUsers(IQueryable<User> query, User currentUser)
        {
            // Parent can only see themselves
            return query.Where(u => u.Id == currentUser.Id);
        }

        private IQueryable<SmsLog> FilterSchoolAdminSmsLogs(IQueryable<SmsLog> query, User user)
        {
            if (!user.SchoolId.HasValue)
            {
                return query.Where(_ => false);
            }

            // School Admin can see SMS logs for their school's applications
            return query.Where(s => _context.Applications.Any(a => a.ParentPhone == s.PhoneNumber && a.SchoolId == user.SchoolId.Value));
        }

        private IQueryable<SmsLog> FilterParentSmsLogs(IQueryable<SmsLog> query, User user)
        {
            // Parent can only see SMS sent to them
            return query.Where(s => s.PhoneNumber == user.PhoneNumber);
        }

        private IQueryable<SecurityAuditLog> FilterSchoolAdminSecurityLogs(IQueryable<SecurityAuditLog> query, User user)
        {
            // School Admin can see security logs related to their school's users
            return query.Where(s => s.UserId == user.Id.ToString() || 
                (s.UserId != null && _context.Users.Any(u => u.Id.ToString() == s.UserId && u.SchoolId == user.SchoolId)));
        }

        private IQueryable<SecurityAuditLog> FilterParentSecurityLogs(IQueryable<SecurityAuditLog> query, User user)
        {
            // Parent can only see their own security logs
            return query.Where(s => s.UserId == user.Id.ToString());
        }

        private IQueryable<T> ApplyGenericFilter<T>(IQueryable<T> query, User user) where T : class
        {
            // For unknown entity types, apply conservative filtering
            _logger.LogWarning("Generic filter applied for unknown entity type {EntityType} by user {UserId}", 
                typeof(T).Name, user.Id);

            // Only Platform Admin gets access to unknown entities
            if (user.Role == UserRoles.PlatformAdmin && UserRoles.CanBePlatformAdmin(user.PhoneNumber))
            {
                return query;
            }

            return query.Where(_ => false); // Deny access to unknown entities for non-platform admins
        }

        private IQueryable<T> LogUnauthorizedAccessAndReturnEmpty<T>(IQueryable<T> query, User user, string entityType) where T : class
        {
            _logger.LogWarning("SECURITY: Unauthorized access attempt to {EntityType} by user {UserId} ({Role})", 
                entityType, user.Id, user.Role);

            // Log security event
            var securityLog = new SecurityAuditLog
            {
                EventType = "UNAUTHORIZED_DATA_ACCESS",
                Details = $"User {user.Id} ({user.Role}) attempted to access {entityType}",
                UserId = user.Id.ToString(),
                Timestamp = DateTimeOffset.UtcNow,
                Success = false
            };

            _context.SecurityAuditLogs.Add(securityLog);
            _context.SaveChangesAsync(); // Fire and forget

            return query.Where(_ => false); // Return empty result
        }

        // Entity-specific access validation methods

        private async Task<bool> CanAccessApplicationAsync(User user, int applicationId)
        {
            var application = await _context.Applications.FindAsync(applicationId);
            if (application == null) return false;

            return user.Role switch
            {
                UserRoles.PlatformAdmin => UserRoles.CanBePlatformAdmin(user.PhoneNumber),
                UserRoles.SchoolAdmin => user.SchoolId == application.SchoolId,
                UserRoles.Parent => application.ParentPhone == user.PhoneNumber,
                _ => false
            };
        }

        private async Task<bool> CanAccessDocumentAsync(User user, int documentId)
        {
            var document = await _context.Documents.Include(d => d.Application).FirstOrDefaultAsync(d => d.Id == documentId);
            if (document == null) return false;

            return user.Role switch
            {
                UserRoles.PlatformAdmin => UserRoles.CanBePlatformAdmin(user.PhoneNumber),
                UserRoles.SchoolAdmin => user.SchoolId == document.Application.SchoolId,
                UserRoles.Parent => document.Application.ParentPhone == user.PhoneNumber,
                _ => false
            };
        }

        private async Task<bool> CanAccessSchoolAsync(User user, int schoolId)
        {
            return user.Role switch
            {
                UserRoles.PlatformAdmin => UserRoles.CanBePlatformAdmin(user.PhoneNumber),
                UserRoles.SchoolAdmin => user.SchoolId == schoolId,
                UserRoles.Parent => false, // Parents cannot access school data
                _ => false
            };
        }

        private async Task<bool> CanAccessUserAsync(User user, int userId)
        {
            var targetUser = await _context.Users.FindAsync(userId);
            if (targetUser == null) return false;

            return user.Role switch
            {
                UserRoles.PlatformAdmin => UserRoles.CanBePlatformAdmin(user.PhoneNumber),
                UserRoles.SchoolAdmin => user.SchoolId == targetUser.SchoolId || 
                    (targetUser.Role == UserRoles.Parent && await _context.Applications.AnyAsync(a => a.ParentPhone == targetUser.PhoneNumber && a.SchoolId == user.SchoolId)),
                UserRoles.Parent => user.Id == userId, // Can only access themselves
                _ => false
            };
        }
    }
}