using SOAP.Web.Services.Interfaces;

namespace SOAP.Web.Middleware
{
    public class SecurityAuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISecurityAuditService _securityAuditService;
        private readonly ILogger<SecurityAuditMiddleware> _logger;

        public SecurityAuditMiddleware(RequestDelegate next, ISecurityAuditService securityAuditService, ILogger<SecurityAuditMiddleware> logger)
        {
            _next = next;
            _securityAuditService = securityAuditService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTimeOffset.UtcNow;
            var originalStatusCode = context.Response.StatusCode;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log security event for exceptions
                await LogSecurityEventAsync(context, "EXCEPTION", false, ex.Message);
                throw;
            }
            finally
            {
                // Log security event for sensitive endpoints
                if (IsSensitiveEndpoint(context.Request.Path.Value))
                {
                    var success = context.Response.StatusCode < 400;
                    await LogSecurityEventAsync(context, "ENDPOINT_ACCESS", success);
                }
            }
        }

        private async Task LogSecurityEventAsync(HttpContext context, string eventType, bool success, string failureReason = null)
        {
            try
            {
                var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
                var userRole = context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "anonymous";
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var userAgent = context.Request.Headers["User-Agent"].ToString() ?? "unknown";

                var securityEvent = new SecurityEvent
                {
                    EventType = eventType,
                    UserId = userId,
                    UserRole = userRole,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    ResourceAccessed = context.Request.Path.Value ?? "",
                    ActionPerformed = $"{context.Request.Method} {context.Request.Path}",
                    Success = success,
                    FailureReason = failureReason ?? ""
                };

                await _securityAuditService.LogSecurityEventAsync(securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security event");
            }
        }

        private bool IsSensitiveEndpoint(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            var sensitiveEndpoints = new[]
            {
                "/Account/Login",
                "/Account/Logout",
                "/Parent/Document/Upload",
                "/Admin/Application/",
                "/Admin/Dashboard/",
                "/api/"
            };

            return sensitiveEndpoints.Any(endpoint => path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static class SecurityAuditMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityAudit(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityAuditMiddleware>();
        }
    }
} 