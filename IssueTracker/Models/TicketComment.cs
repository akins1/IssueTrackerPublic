using System.ComponentModel;

namespace IssueTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        public string Comment { get; set; }

        [DisplayName("Date")]
        public DateTimeOffset CreatedOn { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        [DisplayName("Team Member")]
        public string UserId { get; set; }


        // Navigation properties
        public virtual Ticket Ticket { get; set; }
        public virtual IssueTrackerUser User { get; set; }
    }
}
