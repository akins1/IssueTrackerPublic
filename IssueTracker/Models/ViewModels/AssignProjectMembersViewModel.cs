using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssueTracker.Models.ViewModels
{
    public class AssignProjectMembersViewModel
    {
        public Project Project { get; set; }

        public MultiSelectList Users { get; set; }

        public List<string> SelectedUsers { get; set; }
    }
}
