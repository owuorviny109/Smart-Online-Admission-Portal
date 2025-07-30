using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Areas.Parent.ViewModels
{
    public class DocumentUploadViewModel
    {
        public int ApplicationId { get; set; }

        [Required]
        [Display(Name = "Document Type")]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Document File")]
        public IFormFile DocumentFile { get; set; } = null!;

        public List<DocumentViewModel> ExistingDocuments { get; set; } = new();
    }

    public class DocumentViewModel
    {
        public int Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string UploadStatus { get; set; } = string.Empty;
        public string? AdminFeedback { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}