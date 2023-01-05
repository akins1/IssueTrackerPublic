using System.ComponentModel;
using System.Net.Sockets;

namespace IssueTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        [DisplayName("Updated Item")]
        public string Property { get; set; }

        [DisplayName("Previous")]
        public string PreviousValue { get; set; }

        [DisplayName("Current")]
        public string CurrentValue { get; set; }

        [DisplayName("Date Modified")]
        public DateTimeOffset CreatedOn { get; set; }

        [DisplayName("Description of Change")]
        public string Description { get; set; }

        [DisplayName("Team Member")]
        public string UserId { get; set; }


        // Navigation properties
        public virtual Ticket Ticket { get; set; }

        public virtual IssueTrackerUser User { get; set; }
    }
}
