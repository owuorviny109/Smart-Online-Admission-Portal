using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;
using System.Security.Claims;

namespace SOAP.Web.Authorization
{
    public class ParentDataAccessRequirementHandler : AuthorizationHandler<ParentDataAccessRequirement>
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ParentDataAccessRequirementHandler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ParentDataAccessRequirement requirement)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Fail();
                return;
            }

            // Admin users can access all data within their school
            if (userRole == "SchoolAdmin" || userRole == "SuperAdmin")
            {
                context.Succeed(requirement);
                return;
            }

            // Parents can only access their own data
            if (userRole == "Parent")
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var requestedApplicationId = GetApplicationIdFromRequest(httpContext);

                if (requestedApplicationId == null)
                {
                    context.Fail();
                    return;
                }

                // Get user's phone number
                var userPhone = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.PhoneNumber)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(userPhone))
                {
                    context.Fail();
                    return;
                }

                // Check if the application belongs to this parent
                var applicationExists = await _context.Applications
                    .AnyAsync(a => a.Id == requestedApplicationId && a.ParentPhone == userPhone);

                if (!applicationExists)
                {
                    context.Fail();
                    return;
                }

                context.Succeed(requirement);
                return;
            }

            context.Fail();
        }

        private int? GetApplicationIdFromRequest(HttpContext httpContext)
        {
            // Try to get application ID from route values
            if (httpContext.Request.RouteValues.TryGetValue("applicationId", out var routeAppId))
            {
                if (int.TryParse(routeAppId?.ToString(), out var appId))
                    return appId;
            }

            // Try to get application ID from query parameters
            if (httpContext.Request.Query.TryGetValue("applicationId", out var queryAppId))
            {
                if (int.TryParse(queryAppId.FirstOrDefault(), out var appId))
                    return appId;
            }

            // For document requests, get application ID from document
            if (httpContext.Request.RouteValues.TryGetValue("documentId", out var docId))
            {
                if (int.TryParse(docId?.ToString(), out var documentId))
                {
                    var applicationId = _context.Documents
                        .Where(d => d.Id == documentId)
                        .Select(d => d.ApplicationId)
                        .FirstOrDefault();
                    return applicationId;
                }
            }

            return null;
        }
    }
}