using SOAP.Web.Models.Entities;

namespace SOAP.Web.Services.Interfaces
{
    /// <summary>
    /// Service for applying role-based data filtering to prevent unauthorized access
    /// </summary>
    public interface IDataFilterService
    {
        /// <summary>
        /// Applies user-specific filtering to any queryable data source
        /// SECURITY: Automatically filters data based on user role and permissions
        /// </summary>
        IQueryable<T> ApplyUserFilter<T>(IQueryable<T> query, User user) where T : class;

        /// <summary>
        /// Filters applications based on user role
        /// Platform Admin: All applications
        /// School Admin: Only their school's applications
        /// Parent: Only their own applications
        /// </summary>
        IQueryable<Application> FilterApplications(IQueryable<Application> query, User user);

        /// <summary>
        /// Filters documents based on user role
        /// Platform Admin: All documents (for system management)
        /// School Admin: Only documents for their school's applications
        /// Parent: Only their own documents
        /// </summary>
        IQueryable<Document> FilterDocuments(IQueryable<Document> query, User user);

        /// <summary>
        /// Filters schools based on user role
        /// Platform Admin: All schools
        /// School Admin: Only their own school
        /// Parent: No access to school data
        /// </summary>
        IQueryable<School> FilterSchools(IQueryable<School> query, User user);

        /// <summary>
        /// Filters users based on role and permissions
        /// Platform Admin: All users (for system management)
        /// School Admin: Only users related to their school
        /// Parent: Only themselves
        /// </summary>
        IQueryable<User> FilterUsers(IQueryable<User> query, User currentUser);

        /// <summary>
        /// Filters SMS logs based on user role
        /// Platform Admin: All SMS logs
        /// School Admin: Only SMS sent for their school's applications
        /// Parent: Only SMS sent to them
        /// </summary>
        IQueryable<SmsLog> FilterSmsLogs(IQueryable<SmsLog> query, User user);

        /// <summary>
        /// Filters security audit logs based on user role
        /// Platform Admin: All security logs
        /// School Admin: Only logs related to their school
        /// Parent: Only their own security events
        /// </summary>
        IQueryable<SecurityAuditLog> FilterSecurityLogs(IQueryable<SecurityAuditLog> query, User user);

        /// <summary>
        /// SECURITY: Validates if user can access specific entity by ID
        /// </summary>
        Task<bool> CanAccessEntityAsync<T>(User user, int entityId) where T : class;

        /// <summary>
        /// SECURITY: Gets the appropriate data scope for a user
        /// Used for dashboard statistics and reporting
        /// </summary>
        DataScope GetUserDataScope(User user);

        /// <summary>
        /// SECURITY: Validates and logs data access attempts
        /// </summary>
        Task<bool> ValidateAndLogDataAccessAsync<T>(User user, string operation, int? entityId = null) where T : class;
    }

    /// <summary>
    /// Defines the scope of data a user can access
    /// </summary>
    public class DataScope
    {
        public bool CanAccessAllSchools { get; set; }
        public int? RestrictedToSchoolId { get; set; }
        public bool CanAccessAllUsers { get; set; }
        public string? RestrictedToPhoneNumber { get; set; }
        public bool CanAccessSystemLogs { get; set; }
        public bool CanAccessBillingData { get; set; }
        public List<string> AllowedOperations { get; set; } = new();
    }
}