using System.ComponentModel;

namespace IssueTracker.Models
{
    public class ProjectPriority
    {
        public int Id { get; set; }

        [DisplayName("Priority")]
        public string Name { get; set; }
    }
}
