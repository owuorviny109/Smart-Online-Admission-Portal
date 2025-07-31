using SOAP.Web.Services.Interfaces;

namespace SOAP.Web.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRateLimitingService _rateLimitingService;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        public RateLimitingMiddleware(RequestDelegate next, IRateLimitingService rateLimitingService, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _rateLimitingService = rateLimitingService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            var endpoint = context.Request.Path.Value;

            // Different limits for different endpoints
            var (maxRequests, window) = GetLimitsForEndpoint(endpoint);

            if (!await _rateLimitingService.IsRequestAllowedAsync(clientId, endpoint, maxRequests, window))
            {
                context.Response.StatusCode = 429; // Too Many Requests
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Rate limit exceeded. Please try again later.\"}");
                return;
            }

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Use IP address as primary identifier
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            // For authenticated users, also include user ID
            var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                return $"{ipAddress}:{userId}";
            }

            return ipAddress;
        }

        private (int maxRequests, TimeSpan window) GetLimitsForEndpoint(string endpoint)
        {
            return endpoint switch
            {
                "/Account/Login" => (5, TimeSpan.FromMinutes(15)),      // Strict for login
                "/api/send-otp" => (3, TimeSpan.FromMinutes(5)),        // Very strict for OTP
                "/Parent/Document/Upload" => (10, TimeSpan.FromMinutes(1)), // Moderate for uploads
                _ => (100, TimeSpan.FromMinutes(1))                     // Default
            };
        }
    }

    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
} 