using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services
{
    public class ITRolesService : IITRolesService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IssueTrackerUser> _userManager;

        public ITRolesService (ApplicationDbContext context, 
                               RoleManager<IdentityRole> roleManager,
                               UserManager<IssueTrackerUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }


        public async Task<bool> AddUserToRoleAsync(IssueTrackerUser user, string roleName)
        {
             return ( await _userManager.AddToRoleAsync(user, roleName) ).Succeeded;
        }

        public async Task<List<IdentityRole>> GetRolesAsync()
        {
            try
            {
                return await _context.Roles.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<string> GetRoleNameByIdAsync(string roleId)
        {
            IdentityRole role = _context.Roles.Find(roleId);

            return ( await _roleManager.GetRoleNameAsync(role) );
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(IssueTrackerUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }   

        public async Task<List<IssueTrackerUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            List<IssueTrackerUser> users = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
            
            return users.Where(u => u.CompanyId == companyId).ToList();
        }

        public async Task<List<IssueTrackerUser>> GetUsersNotInRoleAsync(string roleName, int companyId)
        {
            List<string> userIds = (await _userManager.GetUsersInRoleAsync(roleName)).Select(u => u.Id).ToList();
            List<IssueTrackerUser> users = _context.Users.Where(u => !userIds.Contains(u.Id) && 
                                                                     u.CompanyId == companyId).ToList();
            return users;
        }

        public async Task<bool> IsUserInRoleAsync(IssueTrackerUser user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<bool> RemoveUserFromAllRolesAsync(IssueTrackerUser user, IEnumerable<string> roleNames)
        {
            return ( await _userManager.RemoveFromRolesAsync(user, roleNames) ).Succeeded;
        }

        public async Task<bool> RemoveUserFromRoleAsync(IssueTrackerUser user, string roleName)
        {
            return ( await _userManager.RemoveFromRoleAsync(user, roleName) ).Succeeded;
        }
    }
}
