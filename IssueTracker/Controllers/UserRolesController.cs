using IssueTracker.Extensions;
using IssueTracker.Models;
using IssueTracker.Models.ViewModels;
using IssueTracker.Services;
using IssueTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssueTracker.Controllers
{
    [Authorize]
    public class UserRolesController : Controller
    {
        private readonly IITRolesService _rolesService;
        private readonly IITCompanyInfoService _companyInfoService;

        public UserRolesController(IITRolesService roleService, IITCompanyInfoService companyInfoService)
        {
            _rolesService = roleService;
            _companyInfoService = companyInfoService;
        }
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            List<IssueTrackerUser> users = await _companyInfoService.GetAllMembersAsync(companyId);

            foreach (var user in users)
            {
                ManageUserRolesViewModel viewModel = new();
                viewModel.ITUser = user;

                IEnumerable<string> selected = await _rolesService.GetUserRolesAsync(user);
                viewModel.Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", selected);

                model.Add(viewModel);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            IssueTrackerUser user = (await _companyInfoService.GetAllMembersAsync(companyId))
                .FirstOrDefault(u => u.Id == member.ITUser.Id);

            IEnumerable<string> roles = await _rolesService.GetUserRolesAsync(user);

            string selectedRole = member.SelectedRoles.FirstOrDefault();

            if (!string.IsNullOrEmpty(selectedRole))
            {
                if (await _rolesService.RemoveUserFromAllRolesAsync(user, roles))
                {
                    await _rolesService.AddUserToRoleAsync(user, selectedRole);
                }
            }

            return RedirectToAction(nameof(ManageUserRoles));
        }
    }
}
