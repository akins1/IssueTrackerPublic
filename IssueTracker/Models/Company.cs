using System.ComponentModel;

namespace IssueTracker.Models
{
    public class Company
    {
        public int Id { get; set; }

        [DisplayName("Company Name")]
        public string Name { get; set; }

        [DisplayName("Company Description")]
        public string Description { get; set; }


        // Navigation properties
        public virtual ICollection<IssueTrackerUser> Members { get; set; }
        
        public virtual ICollection<Project> Projects { get; set; }

        public virtual ICollection<Invite> Invites { get; set; }
    }
}
