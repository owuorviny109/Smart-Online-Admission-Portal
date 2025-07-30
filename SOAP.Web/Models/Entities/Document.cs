using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [Required]
        [StringLength(30)]
        public string DocumentType { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }

        [Required]
        public long FileSize { get; set; }

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; }

        [StringLength(20)]
        public string UploadStatus { get; set; } = "Uploaded";

        public string? AdminFeedback { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Application Application { get; set; }
    }
}