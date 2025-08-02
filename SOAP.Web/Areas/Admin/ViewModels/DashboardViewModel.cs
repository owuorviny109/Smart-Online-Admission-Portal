namespace SOAP.Web.Areas.Admin.ViewModels
{
    public class DashboardViewModel
    {
        // Basic Application Stats
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int IncompleteApplications { get; set; }

        // Document Stats
        public int TotalDocuments { get; set; }
        public int VerifiedDocuments { get; set; }
        public int PendingDocuments { get; set; }
        public int RejectedDocuments { get; set; }

        // School & Student Stats
        public int TotalSchools { get; set; }
        public int SchoolsWithData { get; set; }
        public int TotalPlacedStudents { get; set; }
        public int StudentsWithApplications { get; set; }

        // SMS & Communication Stats
        public int TotalSmssSent { get; set; }
        public int SmsDelivered { get; set; }
        public int SmsFailed { get; set; }

        // Security & System Stats
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int SecurityIncidents { get; set; }
        public int LoginAttempts { get; set; }

        // Time-based Data for Charts
        public List<DailyApplicationData> DailyApplications { get; set; } = new();
        public List<DocumentTypeData> DocumentsByType { get; set; } = new();
        public List<SchoolApplicationData> ApplicationsBySchool { get; set; } = new();
        public List<MonthlyTrendData> MonthlyTrends { get; set; } = new();

        // Recent Activity
        public List<RecentActivityItem> RecentActivities { get; set; } = new();
    }

    public class DailyApplicationData
    {
        public DateTime Date { get; set; }
        public int ApplicationCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }

    public class DocumentTypeData
    {
        public string DocumentType { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int VerifiedCount { get; set; }
        public int PendingCount { get; set; }
        public int RejectedCount { get; set; }
    }

    public class SchoolApplicationData
    {
        public string SchoolName { get; set; } = string.Empty;
        public int PlacedStudents { get; set; }
        public int ApplicationsReceived { get; set; }
        public int ApplicationsApproved { get; set; }
        public bool HasPlacementData { get; set; }
    }

    public class MonthlyTrendData
    {
        public string Month { get; set; } = string.Empty;
        public int Applications { get; set; }
        public int Documents { get; set; }
        public int SmssSent { get; set; }
    }

    public class RecentActivityItem
    {
        public string Type { get; set; } = string.Empty; // Application, Document, SMS, System
        public string Description { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}