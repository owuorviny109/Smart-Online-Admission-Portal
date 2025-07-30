namespace SOAP.Web.Areas.Admin.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int PendingApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int TodayApplications { get; set; }
        
        public List<RecentApplicationInfo> RecentApplications { get; set; } = new List<RecentApplicationInfo>();
    }

    public class RecentApplicationInfo
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string KcpeIndexNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}