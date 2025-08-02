namespace SOAP.Web.ViewModels
{
    /// <summary>
    /// Platform Admin dashboard view model with system-wide metrics
    /// SaaS-compliant with anonymized data
    /// </summary>
    public class PlatformDashboardViewModel
    {
        // System Health Metrics
        public SystemHealthMetrics SystemHealth { get; set; } = new();
        
        // School Account Management
        public List<SchoolAccountSummary> SchoolAccounts { get; set; } = new();
        public int TotalActiveSchools { get; set; }
        public int TotalSuspendedSchools { get; set; }
        public int NewSchoolsThisMonth { get; set; }
        
        // Aggregated Usage Metrics (Anonymized)
        public AggregatedUsageMetrics UsageMetrics { get; set; } = new();
        public List<PlatformTrendData> PlatformTrends { get; set; } = new();
        
        // Billing and Subscriptions
        public BillingOverview BillingOverview { get; set; } = new();
        
        // Security Overview
        public SecurityOverview SecurityOverview { get; set; } = new();
        
        // System Performance
        public List<PerformanceMetric> PerformanceMetrics { get; set; } = new();
        
        // Recent Platform Events
        public List<PlatformEvent> RecentEvents { get; set; } = new();
    }

    /// <summary>
    /// System health monitoring data
    /// </summary>
    public class SystemHealthMetrics
    {
        public double CpuUsagePercent { get; set; }
        public double MemoryUsagePercent { get; set; }
        public double DiskUsagePercent { get; set; }
        public int ActiveConnections { get; set; }
        public TimeSpan SystemUptime { get; set; }
        public string DatabaseStatus { get; set; } = "Healthy";
        public string EmailServiceStatus { get; set; } = "Healthy";
        public string SmsServiceStatus { get; set; } = "Healthy";
        public DateTime LastHealthCheck { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// School account summary (anonymized for privacy)
    /// </summary>
    public class SchoolAccountSummary
    {
        public int SchoolId { get; set; }
        public string SchoolCode { get; set; } = string.Empty; // Anonymized
        public string County { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Active, Suspended, etc.
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public int TotalApplications { get; set; }
        public int ActiveUsers { get; set; }
        public decimal MonthlyUsage { get; set; } // For billing
        public string SubscriptionTier { get; set; } = "Basic";
    }

    /// <summary>
    /// Aggregated usage metrics across all schools
    /// </summary>
    public class AggregatedUsageMetrics
    {
        public int TotalApplicationsToday { get; set; }
        public int TotalApplicationsThisMonth { get; set; }
        public int TotalDocumentsUploaded { get; set; }
        public int TotalSmssSent { get; set; }
        public int TotalActiveUsers { get; set; }
        public double AverageApplicationsPerSchool { get; set; }
        public double SystemUtilizationPercent { get; set; }
    }

    /// <summary>
    /// Platform-wide trend data
    /// </summary>
    public class PlatformTrendData
    {
        public string Period { get; set; } = string.Empty; // "2024-01", "Week 1", etc.
        public int TotalApplications { get; set; }
        public int TotalDocuments { get; set; }
        public int TotalSms { get; set; }
        public int ActiveSchools { get; set; }
        public int NewUsers { get; set; }
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// Billing and subscription overview
    /// </summary>
    public class BillingOverview
    {
        public decimal TotalMonthlyRevenue { get; set; }
        public decimal ProjectedAnnualRevenue { get; set; }
        public int PaidSubscriptions { get; set; }
        public int TrialSubscriptions { get; set; }
        public int OverdueAccounts { get; set; }
        public List<RevenueByTier> RevenueBreakdown { get; set; } = new();
        public List<MonthlyRevenue> RevenueHistory { get; set; } = new();
    }

    /// <summary>
    /// Revenue breakdown by subscription tier
    /// </summary>
    public class RevenueByTier
    {
        public string TierName { get; set; } = string.Empty;
        public int SchoolCount { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal AverageRevenuePerSchool { get; set; }
    }

    /// <summary>
    /// Monthly revenue tracking
    /// </summary>
    public class MonthlyRevenue
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int NewSubscriptions { get; set; }
        public int Cancellations { get; set; }
    }

    /// <summary>
    /// Security overview for Platform Admin
    /// </summary>
    public class SecurityOverview
    {
        public int SecurityIncidentsToday { get; set; }
        public int SecurityIncidentsThisMonth { get; set; }
        public int FailedLoginAttempts { get; set; }
        public int BlockedIpAddresses { get; set; }
        public int SuspiciousActivities { get; set; }
        public List<SecurityThreat> RecentThreats { get; set; } = new();
        public DateTime LastSecurityScan { get; set; }
        public string SecurityStatus { get; set; } = "Secure";
    }

    /// <summary>
    /// Security threat information
    /// </summary>
    public class SecurityThreat
    {
        public string ThreatType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
        public DateTime DetectedAt { get; set; }
        public string Status { get; set; } = string.Empty; // Active, Mitigated, Resolved
        public string IpAddress { get; set; } = string.Empty;
    }

    /// <summary>
    /// System performance metrics
    /// </summary>
    public class PerformanceMetric
    {
        public string MetricName { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public double AverageValue { get; set; }
        public double MaxValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Good, Warning, Critical
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Platform-level events for audit trail
    /// </summary>
    public class PlatformEvent
    {
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty; // System, User, External
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}