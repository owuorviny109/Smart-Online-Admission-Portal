using SOAP.Web.Models.Entities;

namespace SOAP.Web.Areas.Admin.ViewModels
{
    public class ApplicationReviewViewModel
    {
        public int ApplicationId { get; set; }
        public Application? Application { get; set; }
        public List<Document> Documents { get; set; } = new List<Document>();
        public string Decision { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }

    public class ApplicationListViewModel
    {
        public List<ApplicationSummary> Applications { get; set; } = new List<ApplicationSummary>();
        public string SearchTerm { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class ApplicationSummary
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string KcpeIndexNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int DocumentCount { get; set; }
    }
}