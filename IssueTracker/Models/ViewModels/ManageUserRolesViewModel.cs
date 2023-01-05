using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssueTracker.Models.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public IssueTrackerUser ITUser { get; set; }

        public MultiSelectList Roles { get; set; }

        public List<string> SelectedRoles { get; set; }
    }
}
