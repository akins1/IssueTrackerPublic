namespace IssueTracker.Models.ViewModels
{
    public class DashboardViewModel
    {
        public Company Company { get; set; }

        public List<Project> Projects { get; set; }

        public List<Ticket> Tickets { get; set; }

        public List<IssueTrackerUser> Members { get; set; }

        public List<Project> UserProjects { get; set; }

        public List<Ticket> UserTickets { get; set; }
    }
}
