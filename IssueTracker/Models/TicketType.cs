using MessagePack;
using System.ComponentModel;

namespace IssueTracker.Models
{
    public class TicketType
    {
        public int Id { get; set; }


        [DisplayName("Ticket Type")]
        public string Name { get; set; }

    }
}
