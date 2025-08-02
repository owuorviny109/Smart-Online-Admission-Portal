using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;
using SOAP.Web.Services.Interfaces;
using SOAP.Web.Utilities.Constants;
using SOAP.Web.ViewModels;
using System.Diagnostics;

namespace SOAP.Web.Controllers
{
    /// <summary>
    /// Platform Admin root dashboard controller
    /// SECURITY: Only accessible by verified Platform Admin
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDataFilterService _dataFilterService;
        private readonly IAdvancedSecurityService _securityService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ApplicationDbContext context,
            IDataFilterService dataFilterService,
            IAdvancedSecurityService securityService,
            ILogger<DashboardController> logger)
        {
            _context = context;
            _dataFilterService = dataFilterService;
            _securityService = securityService;
            _logger = logger;
        }

        /// <summary>
        /// Platform Admin dashboard - system-wide overview
        /// SECURITY: Triple validation for Platform Admin access
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // SECURITY CHECK 1: Get current user
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    _logger.LogWarning("Dashboard access attempted with no authenticated user");
                    return RedirectToAction("Login", "Account");
                }

                // SECURITY CHECK 2: Validate Platform Admin role
                if (currentUser.Role != UserRoles.PlatformAdmin)
                {
                    _logger.LogWarning("SECURITY: Non-Platform Admin attempted dashboard access: User {UserId} ({Role})", 
                        currentUser.Id, currentUser.Role);
                    
                    await _securityService.LogSecurityEventAsync("UNAUTHORIZED_PLATFORM_ACCESS", 
                        $"User {currentUser.Id} ({currentUser.Role}) attempted Platform Admin dashboard access", 
                        currentUser, HttpContext.Connection.RemoteIpAddress?.ToString());
                    
                    return RedirectToAction("AccessDenied", "Account");
                }

                // SECURITY CHECK 3: Validate Platform Admin legitimacy
                if (!UserRoles.CanBePlatformAdmin(currentUser.PhoneNumber))
                {
                    _logger.LogCritical("SECURITY VIOLATION: Fake Platform Admin detected: User {UserId}, Phone {Phone}", 
                        currentUser.Id, currentUser.PhoneNumber);
                    
                    await _securityService.SendSecurityAlertAsync("FAKE_PLATFORM_ADMIN_DETECTED", 
                        $"Fake Platform Admin detected: User {currentUser.Id}, Phone {currentUser.PhoneNumber}",
                        $"IP: {HttpContext.Connection.RemoteIpAddress}\nUserAgent: {HttpContext.Request.Headers.UserAgent}");
                    
                    return RedirectToAction("Login", "Account");
                }

                // Log legitimate Platform Admin access
                _logger.LogInformation("Platform Admin dashboard accessed by User {UserId}", currentUser.Id);
                await _securityService.LogSecurityEventAsync("PLATFORM_ADMIN_DASHBOARD_ACCESS", 
                    $"Platform Admin {currentUser.Id} accessed dashboard", 
                    currentUser, HttpContext.Connection.RemoteIpAddress?.ToString());

                // Build dashboard data
                var viewModel = await BuildPlatformDashboardAsync();
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Platform Admin dashboard");
                return View("Error");
            }
        }

        /// <summary>
        /// System health check endpoint
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SystemHealth()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser?.Role != UserRoles.PlatformAdmin || !UserRoles.CanBePlatformAdmin(currentUser.PhoneNumber))
            {
                return Forbid();
            }

            var healthMetrics = await GetSystemHealthMetricsAsync();
            return Json(healthMetrics);
        }

        /// <summary>
        /// School management endpoint
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SchoolAccounts()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser?.Role != UserRoles.PlatformAdmin || !UserRoles.CanBePlatformAdmin(currentUser.PhoneNumber))
            {
                return Forbid();
            }

            var schoolAccounts = await GetSchoolAccountSummariesAsync();
            return Json(schoolAccounts);
        }

        /// <summary>
        /// Security overview endpoint
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SecurityOverview()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser?.Role != UserRoles.PlatformAdmin || !UserRoles.CanBePlatformAdmin(currentUser.PhoneNumber))
            {
                return Forbid();
            }

            var securityOverview = await GetSecurityOverviewAsync();
            return Json(securityOverview);
        }

        /// <summary>
        /// Suspend school account (emergency action)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SuspendSchool(int schoolId, string reason)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser?.Role != UserRoles.PlatformAdmin || !UserRoles.CanBePlatformAdmin(currentUser.PhoneNumber))
            {
                return Forbid();
            }

            try
            {
                var school = await _context.Schools.FindAsync(schoolId);
                if (school == null)
                {
                    return NotFound();
                }

                school.IsActive = false;
                school.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                // Log the action
                await _securityService.LogSecurityEventAsync("SCHOOL_SUSPENDED", 
                    $"School {schoolId} suspended by Platform Admin {currentUser.Id}. Reason: {reason}", 
                    currentUser);

                // Send alert
                await _securityService.SendSecurityAlertAsync("SCHOOL_SUSPENDED", 
                    $"School {school.Name} ({school.Code}) has been suspended",
                    $"Reason: {reason}\nSuspended by: Platform Admin {currentUser.Id}");

                return Json(new { success = true, message = "School suspended successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending school {SchoolId}", schoolId);
                return Json(new { success = false, message = "Error suspending school" });
            }
        }

        // Private helper methods

        private async Task<Models.Entities.User?> GetCurrentUserAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return null;

            var phoneNumber = User.Identity.Name; // Assuming phone number is used as username
            if (string.IsNullOrEmpty(phoneNumber))
                return null;

            return await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber && u.IsActive);
        }

        private async Task<PlatformDashboardViewModel> BuildPlatformDashboardAsync()
        {
            var viewModel = new PlatformDashboardViewModel
            {
                SystemHealth = await GetSystemHealthMetricsAsync(),
                SchoolAccounts = await GetSchoolAccountSummariesAsync(),
                UsageMetrics = await GetAggregatedUsageMetricsAsync(),
                PlatformTrends = await GetPlatformTrendsAsync(),
                BillingOverview = await GetBillingOverviewAsync(),
                SecurityOverview = await GetSecurityOverviewAsync(),
                PerformanceMetrics = await GetPerformanceMetricsAsync(),
                RecentEvents = await GetRecentPlatformEventsAsync()
            };

            // Calculate summary statistics
            viewModel.TotalActiveSchools = viewModel.SchoolAccounts.Count(s => s.Status == "Active");
            viewModel.TotalSuspendedSchools = viewModel.SchoolAccounts.Count(s => s.Status == "Suspended");
            viewModel.NewSchoolsThisMonth = viewModel.SchoolAccounts.Count(s => s.CreatedAt >= DateTime.UtcNow.AddDays(-30));

            return viewModel;
        }

        private async Task<SystemHealthMetrics> GetSystemHealthMetricsAsync()
        {
            // Get system performance metrics
            var process = Process.GetCurrentProcess();
            var totalMemory = GC.GetTotalMemory(false);
            
            return new SystemHealthMetrics
            {
                CpuUsagePercent = await GetCpuUsageAsync(),
                MemoryUsagePercent = (totalMemory / (1024.0 * 1024.0 * 1024.0)) * 100, // Convert to GB percentage
                DiskUsagePercent = await GetDiskUsageAsync(),
                ActiveConnections = await GetActiveConnectionsAsync(),
                SystemUptime = DateTime.UtcNow - process.StartTime,
                DatabaseStatus = await CheckDatabaseHealthAsync(),
                EmailServiceStatus = await CheckEmailServiceHealthAsync(),
                SmsServiceStatus = await CheckSmsServiceHealthAsync(),
                LastHealthCheck = DateTime.UtcNow
            };
        }

        private async Task<List<SchoolAccountSummary>> GetSchoolAccountSummariesAsync()
        {
            return await _context.Schools
                .Select(s => new SchoolAccountSummary
                {
                    SchoolId = s.Id,
                    SchoolCode = s.Code,
                    County = s.County,
                    Status = s.IsActive ? "Active" : "Suspended",
                    CreatedAt = s.CreatedAt,
                    LastActivity = s.UpdatedAt,
                    TotalApplications = _context.Applications.Count(a => a.SchoolId == s.Id),
                    ActiveUsers = _context.Users.Count(u => u.SchoolId == s.Id && u.IsActive),
                    MonthlyUsage = CalculateMonthlyUsage(s.Id),
                    SubscriptionTier = "Basic" // TODO: Implement subscription tiers
                })
                .OrderByDescending(s => s.LastActivity)
                .ToListAsync();
        }

        private async Task<AggregatedUsageMetrics> GetAggregatedUsageMetricsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            return new AggregatedUsageMetrics
            {
                TotalApplicationsToday = await _context.Applications.CountAsync(a => a.CreatedAt.Date == today),
                TotalApplicationsThisMonth = await _context.Applications.CountAsync(a => a.CreatedAt >= thisMonth),
                TotalDocumentsUploaded = await _context.Documents.CountAsync(),
                TotalSmssSent = await _context.SmsLogs.CountAsync(),
                TotalActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                AverageApplicationsPerSchool = await CalculateAverageApplicationsPerSchoolAsync(),
                SystemUtilizationPercent = await CalculateSystemUtilizationAsync()
            };
        }

        private async Task<List<PlatformTrendData>> GetPlatformTrendsAsync()
        {
            var trends = new List<PlatformTrendData>();
            var startDate = DateTime.UtcNow.AddMonths(-6);

            for (int i = 0; i < 6; i++)
            {
                var monthStart = startDate.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1);

                var trend = new PlatformTrendData
                {
                    Period = monthStart.ToString("yyyy-MM"),
                    TotalApplications = await _context.Applications
                        .CountAsync(a => a.CreatedAt >= monthStart && a.CreatedAt < monthEnd),
                    TotalDocuments = await _context.Documents
                        .CountAsync(d => d.CreatedAt >= monthStart && d.CreatedAt < monthEnd),
                    TotalSms = await _context.SmsLogs
                        .CountAsync(s => s.CreatedAt >= monthStart && s.CreatedAt < monthEnd),
                    ActiveSchools = await _context.Schools
                        .CountAsync(s => s.IsActive && s.CreatedAt < monthEnd),
                    NewUsers = await _context.Users
                        .CountAsync(u => u.CreatedAt >= monthStart && u.CreatedAt < monthEnd),
                    Revenue = CalculateMonthlyRevenue(monthStart, monthEnd)
                };

                trends.Add(trend);
            }

            return trends;
        }

        private async Task<BillingOverview> GetBillingOverviewAsync()
        {
            // TODO: Implement actual billing logic
            return new BillingOverview
            {
                TotalMonthlyRevenue = 50000, // Placeholder
                ProjectedAnnualRevenue = 600000,
                PaidSubscriptions = await _context.Schools.CountAsync(s => s.IsActive),
                TrialSubscriptions = 0,
                OverdueAccounts = 0,
                RevenueBreakdown = new List<RevenueByTier>
                {
                    new() { TierName = "Basic", SchoolCount = await _context.Schools.CountAsync(), MonthlyRevenue = 50000, AverageRevenuePerSchool = 1000 }
                },
                RevenueHistory = new List<MonthlyRevenue>()
            };
        }

        private async Task<SecurityOverview> GetSecurityOverviewAsync()
        {
            var today = DateTime.UtcNow.Date;
            var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            return new SecurityOverview
            {
                SecurityIncidentsToday = await _context.SecurityAuditLogs
                    .CountAsync(s => s.Timestamp.Date == today && !s.Success),
                SecurityIncidentsThisMonth = await _context.SecurityAuditLogs
                    .CountAsync(s => s.Timestamp >= thisMonth && !s.Success),
                FailedLoginAttempts = await _context.LoginAttempts
                    .CountAsync(l => l.AttemptedAt.Date == today && !l.Success),
                BlockedIpAddresses = await _context.SecurityAuditLogs
                    .Where(s => s.EventType.Contains("BLOCKED"))
                    .Select(s => s.IpAddress)
                    .Distinct()
                    .CountAsync(),
                SuspiciousActivities = await _context.SecurityAuditLogs
                    .CountAsync(s => s.EventType.Contains("VIOLATION") || s.EventType.Contains("SUSPICIOUS")),
                RecentThreats = await GetRecentSecurityThreatsAsync(),
                LastSecurityScan = DateTime.UtcNow.AddHours(-1), // Placeholder
                SecurityStatus = "Secure"
            };
        }

        private async Task<List<PerformanceMetric>> GetPerformanceMetricsAsync()
        {
            return new List<PerformanceMetric>
            {
                new() { MetricName = "Response Time", CurrentValue = 150, AverageValue = 200, MaxValue = 500, Unit = "ms", Status = "Good" },
                new() { MetricName = "Throughput", CurrentValue = 1000, AverageValue = 800, MaxValue = 2000, Unit = "req/min", Status = "Good" },
                new() { MetricName = "Error Rate", CurrentValue = 0.1, AverageValue = 0.5, MaxValue = 5, Unit = "%", Status = "Good" }
            };
        }

        private async Task<List<PlatformEvent>> GetRecentPlatformEventsAsync()
        {
            return await _context.SecurityAuditLogs
                .Where(s => s.EventType.Contains("PLATFORM") || s.EventType.Contains("SYSTEM"))
                .OrderByDescending(s => s.Timestamp)
                .Take(10)
                .Select(s => new PlatformEvent
                {
                    EventType = s.EventType,
                    Description = s.Details,
                    Timestamp = s.Timestamp.DateTime,
                    Severity = s.Success ? "Info" : "Warning",
                    Source = "System"
                })
                .ToListAsync();
        }

        // Helper methods for system metrics
        private async Task<double> GetCpuUsageAsync()
        {
            // Simplified CPU usage calculation
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            await Task.Delay(500);
            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
        }

        private async Task<double> GetDiskUsageAsync()
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:");
                var usedSpace = drive.TotalSize - drive.AvailableFreeSpace;
                return (double)usedSpace / drive.TotalSize * 100;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> GetActiveConnectionsAsync()
        {
            // Placeholder - would need actual connection pool monitoring
            return 25;
        }

        private async Task<string> CheckDatabaseHealthAsync()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                return "Healthy";
            }
            catch
            {
                return "Unhealthy";
            }
        }

        private async Task<string> CheckEmailServiceHealthAsync()
        {
            // TODO: Implement actual email service health check
            return "Healthy";
        }

        private async Task<string> CheckSmsServiceHealthAsync()
        {
            // TODO: Implement actual SMS service health check
            return "Healthy";
        }

        private decimal CalculateMonthlyUsage(int schoolId)
        {
            // TODO: Implement actual usage calculation based on applications, SMS, storage, etc.
            return 1000; // Placeholder
        }

        private async Task<double> CalculateAverageApplicationsPerSchoolAsync()
        {
            var totalSchools = await _context.Schools.CountAsync(s => s.IsActive);
            var totalApplications = await _context.Applications.CountAsync();
            return totalSchools > 0 ? (double)totalApplications / totalSchools : 0;
        }

        private async Task<double> CalculateSystemUtilizationAsync()
        {
            // TODO: Implement actual system utilization calculation
            return 65.5; // Placeholder
        }

        private decimal CalculateMonthlyRevenue(DateTime monthStart, DateTime monthEnd)
        {
            // TODO: Implement actual revenue calculation
            return 45000; // Placeholder
        }

        private async Task<List<SecurityThreat>> GetRecentSecurityThreatsAsync()
        {
            return await _context.SecurityAuditLogs
                .Where(s => !s.Success && (s.EventType.Contains("VIOLATION") || s.EventType.Contains("ATTACK")))
                .OrderByDescending(s => s.Timestamp)
                .Take(5)
                .Select(s => new SecurityThreat
                {
                    ThreatType = s.EventType,
                    Description = s.Details,
                    Severity = s.EventType.Contains("CRITICAL") ? "Critical" : "Medium",
                    DetectedAt = s.Timestamp.DateTime,
                    Status = "Mitigated",
                    IpAddress = s.IpAddress ?? "Unknown"
                })
                .ToListAsync();
        }
    }
}