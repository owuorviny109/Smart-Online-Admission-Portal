using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SOAP.Web.Configuration;
using SOAP.Web.Services.Interfaces;

namespace SOAP.Web.Services
{
    public class RateLimitingService : IRateLimitingService
    {
        private readonly IMemoryCache _cache;
        private readonly SecurityConfig _securityConfig;
        private readonly ILogger<RateLimitingService> _logger;

        public RateLimitingService(IMemoryCache cache, IOptions<SecurityConfig> securityConfig, ILogger<RateLimitingService> logger)
        {
            _cache = cache;
            _securityConfig = securityConfig.Value;
            _logger = logger;
        }

        public async Task<bool> IsRequestAllowedAsync(string clientId, string endpoint, int maxRequests, TimeSpan window)
        {
            var cacheKey = $"rate_limit:{clientId}:{endpoint}";
            var requestCount = await GetRequestCountAsync(cacheKey);

            if (requestCount >= maxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for {ClientId} on {Endpoint}", clientId, endpoint);
                return false;
            }

            await IncrementRequestCountAsync(clientId, endpoint);
            return true;
        }

        public async Task<bool> IsLoginAllowedAsync(string phoneNumber)
        {
            return await IsRequestAllowedAsync(
                phoneNumber, 
                "login", 
                _securityConfig.RateLimiting.LoginMaxRequests, 
                TimeSpan.FromMinutes(_securityConfig.RateLimiting.LoginWindowMinutes));
        }

        public async Task<bool> IsOtpRequestAllowedAsync(string phoneNumber)
        {
            return await IsRequestAllowedAsync(
                phoneNumber, 
                "otp", 
                _securityConfig.RateLimiting.OtpMaxRequests, 
                TimeSpan.FromMinutes(_securityConfig.RateLimiting.OtpWindowMinutes));
        }

        public async Task<bool> IsUploadAllowedAsync(string userId)
        {
            return await IsRequestAllowedAsync(
                userId, 
                "upload", 
                _securityConfig.RateLimiting.UploadMaxRequests, 
                TimeSpan.FromMinutes(_securityConfig.RateLimiting.UploadWindowMinutes));
        }

        public async Task IncrementRequestCountAsync(string clientId, string endpoint)
        {
            var cacheKey = $"rate_limit:{clientId}:{endpoint}";
            var currentCount = await GetRequestCountAsync(cacheKey);
            
            var window = GetWindowForEndpoint(endpoint);
            _cache.Set(cacheKey, currentCount + 1, window);
        }

        public async Task ResetRequestCountAsync(string clientId, string endpoint)
        {
            var cacheKey = $"rate_limit:{clientId}:{endpoint}";
            _cache.Remove(cacheKey);
            await Task.CompletedTask;
        }

        private async Task<int> GetRequestCountAsync(string cacheKey)
        {
            if (_cache.TryGetValue(cacheKey, out int count))
            {
                return count;
            }
            return 0;
        }

        private TimeSpan GetWindowForEndpoint(string endpoint)
        {
            return endpoint switch
            {
                "login" => TimeSpan.FromMinutes(_securityConfig.RateLimiting.LoginWindowMinutes),
                "otp" => TimeSpan.FromMinutes(_securityConfig.RateLimiting.OtpWindowMinutes),
                "upload" => TimeSpan.FromMinutes(_securityConfig.RateLimiting.UploadWindowMinutes),
                _ => TimeSpan.FromMinutes(1)
            };
        }
    }
} 