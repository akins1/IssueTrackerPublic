using IssueTracker.Models;

namespace IssueTracker.Models.ViewModels
{
    public class SearchViewModel
    {
        public List<Company> Companies { get; set; }

        public List<IssueTrackerUser> Users { get; set; }

        public List<Project> Projects { get; set; }

        public List<Ticket> Tickets { get; set; }
    }
}
