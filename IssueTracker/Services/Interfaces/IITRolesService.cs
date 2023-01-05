using IssueTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace IssueTracker.Services.Interfaces
{
    public interface IITRolesService
    {
        public Task<bool> IsUserInRoleAsync (IssueTrackerUser user, string roleName);

        public Task<List<IdentityRole>> GetRolesAsync();

        public Task<IEnumerable<string>> GetUserRolesAsync (IssueTrackerUser user);

        public Task<bool> AddUserToRoleAsync (IssueTrackerUser user, string roleName);

        public Task<bool> RemoveUserFromRoleAsync (IssueTrackerUser user, string roleName);

        public Task<bool> RemoveUserFromAllRolesAsync (IssueTrackerUser user, IEnumerable<string> roleNames);

        public Task<List<IssueTrackerUser>> GetUsersInRoleAsync (string roleName, int companyId);

        public Task<List<IssueTrackerUser>> GetUsersNotInRoleAsync (string roleName, int companyId);

        public Task<string> GetRoleNameByIdAsync (string roleIdId);
    }
}
