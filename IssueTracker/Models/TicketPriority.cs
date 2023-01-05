using System.ComponentModel;

namespace IssueTracker.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }

        [DisplayName("Ticket Priority")]
        public string Name { get; set; }
    }
}
