namespace SOAP.Web.Models.DTOs
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string DocumentType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public string UploadStatus { get; set; }
        public string? AdminFeedback { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}