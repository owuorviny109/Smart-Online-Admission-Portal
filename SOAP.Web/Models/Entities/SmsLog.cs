using System.ComponentModel.DataAnnotations;

namespace SOAP.Web.Models.Entities
{
    public class SmsLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(20)]
        public string MessageType { get; set; } // Incoming, Outgoing

        [Required]
        public string Content { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Sent"; // Sent, Delivered, Failed

        public int? ApplicationId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Application? Application { get; set; }
    }
}