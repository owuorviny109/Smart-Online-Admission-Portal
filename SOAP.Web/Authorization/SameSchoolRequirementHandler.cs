using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;
using System.Security.Claims;

namespace SOAP.Web.Authorization
{
    public class SameSchoolRequirementHandler : AuthorizationHandler<SameSchoolRequirement>
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SameSchoolRequirementHandler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            SameSchoolRequirement requirement)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
            {
                context.Fail();
                return;
            }

            // SuperAdmin can access all schools
            if (userRole == "SuperAdmin")
            {
                context.Succeed(requirement);
                return;
            }

            // Get the school ID from the route or query parameters
            var httpContext = _httpContextAccessor.HttpContext;
            var requestedSchoolId = GetSchoolIdFromRequest(httpContext);

            if (requestedSchoolId == null)
            {
                context.Fail();
                return;
            }

            // Get user's school ID
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.SchoolId)
                .FirstOrDefaultAsync();

            if (user == null || user != requestedSchoolId)
            {
                context.Fail();
                return;
            }

            context.Succeed(requirement);
        }

        private int? GetSchoolIdFromRequest(HttpContext httpContext)
        {
            // Try to get school ID from route values
            if (httpContext.Request.RouteValues.TryGetValue("schoolId", out var routeSchoolId))
            {
                if (int.TryParse(routeSchoolId?.ToString(), out var schoolId))
                    return schoolId;
            }

            // Try to get school ID from query parameters
            if (httpContext.Request.Query.TryGetValue("schoolId", out var querySchoolId))
            {
                if (int.TryParse(querySchoolId.FirstOrDefault(), out var schoolId))
                    return schoolId;
            }

            // For application-related requests, get school ID from application
            if (httpContext.Request.RouteValues.TryGetValue("applicationId", out var appId))
            {
                if (int.TryParse(appId?.ToString(), out var applicationId))
                {
                    var schoolId = _context.Applications
                        .Where(a => a.Id == applicationId)
                        .Select(a => a.SchoolId)
                        .FirstOrDefault();
                    return schoolId;
                }
            }

            return null;
        }
    }
}