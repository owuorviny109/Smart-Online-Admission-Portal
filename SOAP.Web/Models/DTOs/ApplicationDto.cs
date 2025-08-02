namespace SOAP.Web.Models.DTOs
{
    public class ApplicationDto
    {
        public int Id { get; set; }
        public string KcpeIndexNumber { get; set; }
        public string StudentName { get; set; }
        public int StudentAge { get; set; }
        public string ParentPhone { get; set; }
        public string ParentName { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyName { get; set; }
        public string? HomeAddress { get; set; }
        public string BoardingStatus { get; set; }
        public string? MedicalConditions { get; set; }
        public string Status { get; set; }
        public string? AdmissionCode { get; set; }
        public bool CheckedIn { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        
        public SchoolDto School { get; set; }
        public List<DocumentDto> Documents { get; set; } = new List<DocumentDto>();
    }

    public class SchoolDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string County { get; set; }
    }
}