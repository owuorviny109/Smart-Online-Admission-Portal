using Microsoft.AspNetCore.Mvc;
using SOAP.Web.Services.Interfaces;
using SOAP.Web.Models;
using System.Security.Claims;

namespace SOAP.Web.Controllers
{
    /// <summary>
    /// Base controller implementing common functionality for all controllers
    /// Demonstrates: Inheritance, Abstraction, DIP
    /// </summary>
    public abstract class BaseController : Controller
    {
        protected readonly ILogger _logger;
        protected readonly ISecurityAuditService _auditService;

        protected BaseController(ILogger logger, ISecurityAuditService auditService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        /// <summary>
        /// Logs security events with consistent format
        /// Encapsulation: Protected method for derived classes only
        /// </summary>
        protected virtual async Task LogSecurityEventAsync(string eventType, bool success, string? details = null)
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    EventType = eventType,
                    Success = success,
                    UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UserRole = User.FindFirst(ClaimTypes.Role)?.Value,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault(),
                    ResourceAccessed = $"{HttpContext.Request.Method} {HttpContext.Request.Path}",
                    ActionPerformed = ControllerContext.ActionDescriptor.ActionName,
                    AdditionalData = details != null ? new Dictionary<string, object> { ["details"] = details } : null
                };

                await _auditService.LogSecurityEventAsync(securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security event: {EventType}", eventType);
            }
        }

        /// <summary>
        /// Handles model validation consistently across controllers
        /// Template Method Pattern: Common validation logic
        /// </summary>
        protected virtual IActionResult HandleModelValidation<T>(T model, string viewName = null) where T : class
        {
            if (!ModelState.IsValid)
            {
                LogSecurityEventAsync("VALIDATION_FAILED", false, "Model validation failed").ConfigureAwait(false);
                return View(viewName, model);
            }
            return null;
        }

        /// <summary>
        /// Creates standardized error responses
        /// Encapsulation: Centralized error handling
        /// </summary>
        protected virtual IActionResult CreateErrorResponse(string message, int statusCode = 400)
        {
            LogSecurityEventAsync("ERROR_RESPONSE", false, message).ConfigureAwait(false);
            
            if (Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return StatusCode(statusCode, new { error = message });
            }
            
            TempData["ErrorMessage"] = message;
            return RedirectToAction("Error", "Home");
        }

        /// <summary>
        /// Gets current user information safely
        /// Encapsulation: Protected access to user context
        /// </summary>
        protected virtual string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Gets current user role safely
        /// Encapsulation: Protected access to user role
        /// </summary>
        protected virtual string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Checks if current user has specific role
        /// Abstraction: Simplified role checking
        /// </summary>
        protected virtual bool IsInRole(string role)
        {
            return User.IsInRole(role);
        }

        /// <summary>
        /// Override to add consistent error handling
        /// Template Method Pattern: Consistent error handling across controllers
        /// </summary>
        protected override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            
            // Log action execution
            LogSecurityEventAsync("ACTION_EXECUTING", true, 
                $"Controller: {ControllerContext.ActionDescriptor.ControllerName}, Action: {ControllerContext.ActionDescriptor.ActionName}")
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Override to add consistent success logging
        /// Template Method Pattern: Consistent success handling
        /// </summary>
        protected override void OnActionExecuted(Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            
            if (context.Exception == null)
            {
                LogSecurityEventAsync("ACTION_EXECUTED", true,
                    $"Controller: {ControllerContext.ActionDescriptor.ControllerName}, Action: {ControllerContext.ActionDescriptor.ActionName}")
                    .ConfigureAwait(false);
            }
            else
            {
                LogSecurityEventAsync("ACTION_EXCEPTION", false, context.Exception.Message)
                    .ConfigureAwait(false);
            }
        }
    }
}