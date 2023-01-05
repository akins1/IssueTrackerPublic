using IssueTracker.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Models
{
    public class TicketAttachment
    {
        public Guid Id { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        [DisplayName("File Date")]
        public DateTimeOffset CreatedOn{ get; set; }

        [DisplayName("Team Member")]
        public string UserId { get; set; }

        [DisplayName("File Description")]
        public string Description { get; set; }


        // Attachment information
        [NotMapped]
        [DataType(DataType.Upload)]
        [DisplayName("Select a file")]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] {".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" } )]
        public IFormFile FormFile { get; set; }

        [DisplayName("File Name")]
        public string FileName { get; set; } = "Avatar";

        public byte[] FileData { get; set; } = new byte[0];

        [DisplayName("File Extention")]
        public string FileContentType { get; set; } = "File Extension";


        // Navigation properties
        public virtual Ticket Ticket { get; set; }

        public virtual IssueTrackerUser User { get; set; }
    }
}
