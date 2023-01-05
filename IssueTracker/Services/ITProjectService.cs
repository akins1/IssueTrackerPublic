using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Models.Enums;
using IssueTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration;

namespace IssueTracker.Services
{
    public class ITProjectService : IITProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IITRolesService _roleService;

        public ITProjectService(ApplicationDbContext context, IITRolesService roleService)
        {
            _context = context;
            _roleService = roleService;
        }

        public async Task AddNewProjectAsync(Project project)
        {
            _context.Add(project);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            IssueTrackerUser currentProjectManager = await GetProjectManagerAsync(projectId);

            if (currentProjectManager != null)
            {
                try
                {
                    await RemoveProjectManagerAsync(projectId);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"*** ERROR *** -> Couldn't remove current Project Manager -> Error: {e.Message}");
                    return false;
                }
            }

            Project project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            IssueTrackerUser newProjectManager = await GetUserAsync(userId);
            
            try
            {
                await AddUserToProjectAsync(userId, projectId);

                //is user project Manager?

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"*** ERROR *** -> Couldn't add Project Manager -> Error: {e.Message}");
                return false;
            }
        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            IssueTrackerUser user = await GetUserAsync(userId);

            if (user == null)
            {
                return false;
            }
                
            Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        
            if (await IsUserOnProjectAsync(userId, projectId))
            {
                return false;
            }


            try
            {
                project.Members.Add(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;

                await UpdateProjectAsync(project);

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<IssueTrackerUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            List<IssueTrackerUser> admins = await GetProjectMembersByRoleAsync(projectId, RoleEnum.Admin.ToString());
            List<IssueTrackerUser> developers = await GetProjectMembersByRoleAsync(projectId, RoleEnum.Developer.ToString());
            List<IssueTrackerUser> submitters = await GetProjectMembersByRoleAsync(projectId, RoleEnum.Submitter.ToString());

            List<IssueTrackerUser> projectMembers = admins.Concat(developers).Concat(submitters).ToList();
            return projectMembers;
        }

        public async Task<List<Project>> GetAllProjectsByCompany(int companyId)
        {
            return await _context.Projects.Where(p => p.CompanyId == companyId)
                                          .Include(p => p.Company)
                                          .Include(p => p.ProjectPriority)
                                          .Include(p => p.Members)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.Comments)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.History)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.Attachments)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.TicketPriority)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.TicketStatus)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.TicketType)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.OwnerUser)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.DeveloperUser)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.Notifications)
                                          .ToListAsync();
        }

        public async Task<List<Project>> GetUnassignedProjectsByCompany(int companyId)
        {
            try
            {
                List<Project> projects = (await GetAllProjectsByCompany(companyId)).ToList();

                List<Project> unassignedProjects = new();

                foreach(Project project in projects)
                {
                    if ( (await GetProjectMembersByRoleAsync(project.Id, nameof(RoleEnum.ProjectManager))).Count == 0)
                    {
                        unassignedProjects.Add(project);
                    }
                }

                return unassignedProjects;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName)
        {
            List<Project> projects = await GetAllProjectsByCompany(companyId);

            int priorityId = await LookupProjectPriorityId(priorityName);

            return projects.Where(p => p.ProjectPriorityId == priorityId).ToList();
        }

        public async Task<List<Project>> GetArchivedProjectsByCompany(int companyId)
        {
            List<Project> projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == true)
                                          .Include(p => p.Company)
                                          .Include(p => p.ProjectPriority)
                                          .Include(p => p.Members)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.Comments)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.History)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.Attachments)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.TicketPriority)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.TicketStatus)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.TicketType)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.OwnerUser)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.DeveloperUser)
                                          .Include(p => p.Tickets)
                                            .ThenInclude(t => t.Notifications)
                                          .ToListAsync();

            return projects;
        }

        public Task<List<IssueTrackerUser>> GetDevelopersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            /*return await _context.Projects.Include(p => p.Tickets)
                                              .ThenInclude(t => t.Comments)
                                              .ThenInclude(t => t.)
                                          .Include(p => p.Members)
                                          .Include(p => p.ProjectPriority)
                                          .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
                          */
            return (await GetAllProjectsByCompany(companyId)).Find(p => p.Id == projectId);
        }   

        public async Task<IssueTrackerUser> GetProjectManagerAsync(int projectId)
        {
            Project project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        
            foreach (IssueTrackerUser user in project?.Members)
            {
                if (await _roleService.IsUserInRoleAsync(user, RoleEnum.ProjectManager.ToString()))
                {
                    return user;
                }
            }

            return null;
        }

        public async Task<List<IssueTrackerUser>> GetProjectMembersByRoleAsync(int projectId, string roleName)
        {
            Project project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            List<IssueTrackerUser> members = new();

            foreach (var user in project.Members)
            {
                if (await _roleService.IsUserInRoleAsync(user, roleName))
                {
                    members.Add(user);
                }
            }

            return members;
        }

        public Task<List<IssueTrackerUser>> GetSubmittersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public async Task<IssueTrackerUser> GetUserAsync(string userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            try
            {
                IssueTrackerUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (await _roleService.IsUserInRoleAsync(user, nameof(RoleEnum.Admin)))
                {
                    return await GetAllProjectsByCompany(user.CompanyId.Value);
                } 
                else //if (await _roleService.IsUserInRoleAsync(user, nameof(RoleEnum.ProjectManager)))
                {
                    List<Project> projects = (await _context.Users
                    .Include(u => u.Projects)
                       .ThenInclude(p => p.Company)
                    .Include(u => u.Projects)
                       .ThenInclude(p => p.Members)
                    .Include(u => u.Projects)
                       .ThenInclude(p => p.Tickets)
                    .Include(u => u.Projects)
                       .ThenInclude(p => p.Tickets)
                           .ThenInclude(p => p.DeveloperUser)
                    .Include(u => u.Projects)
                       .ThenInclude(p => p.Tickets)
                           .ThenInclude(p => p.OwnerUser)
                    .Include(u => u.Projects)
                       .ThenInclude(p => p.Tickets)
                           .ThenInclude(p => p.TicketPriority)
                    .Include(u => u.Projects)
                       .ThenInclude(p => p.Tickets)
                           .ThenInclude(p => p.TicketStatus)
                    .Include(u => u.Projects)
                       .ThenInclude(p => p.Tickets)
                           .ThenInclude(p => p.TicketType)
                    .FirstOrDefaultAsync(u => u.Id == userId)).Projects.ToList();

                    return projects;
                }
                
            } 
            catch (Exception e)
            {
                Console.WriteLine($"*** ERROR *** -> Error Removing User from Project. -> {e.Message}");
                throw;
            }
        }

        public async Task<List<IssueTrackerUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            List<IssueTrackerUser> users = await _context.Users
                .Where(u => u.Projects.All(p => p.Id != projectId)).ToListAsync();

            return users.Where(u => u.CompanyId == companyId).ToList();
        }

        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            Project project = await _context.Projects.Include(p => p.Members)
                                                     .FirstOrDefaultAsync(p => p.Id == projectId);
        
            if (project == null)
            {
                return false;
            }

            return project.Members.Any(u => u.Id == userId);
        }

        public async Task<bool> IsUserProjectManagerAsync(string userId, int projectId)
        {
            try
            {
                string projectManagerId = (await GetProjectManagerAsync(projectId))?.Id;

                return projectManagerId == userId;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<int> LookupProjectPriorityId(string priorityName)
        {
            return (await _context.ProjectPriorities.FirstOrDefaultAsync(prio => prio.Name == priorityName)).Id;
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            Project project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            try
            {
                foreach (var user in project?.Members)
                {
                    if (await _roleService.IsUserInRoleAsync(user, RoleEnum.ProjectManager.ToString()))
                    {
                        await RemoveUserFromProjectAsync(user.Id, projectId);
                    }
                }
            }
            catch
            {

            }
        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            try
            {
                IssueTrackerUser user = await GetUserAsync(userId);
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                try
                {
                    if (await IsUserOnProjectAsync(userId, projectId))
                    {
                        project.Members.Remove(user);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"*** ERROR *** - Error Removing User from Project. -> {e.Message}");
            }
        }

        public async Task RemoveUsersFromProjectByRoleAsync(string roleName, int projectId)
        {
            try
            {
                List<IssueTrackerUser> members = await GetProjectMembersByRoleAsync(projectId, roleName);
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (IssueTrackerUser user in members)
                {
                    try
                    {
                        project.Members.Remove(user);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"*** ERROR *** -> Error Removing User from Project by Role. -> {e.Message}");
                throw;
            }
        }

        public async Task RestoreProjectAsync(Project project)
        {
            try
            {
                project.Archived = false;

                await UpdateProjectAsync(project);

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = false;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateProjectAsync(Project project)
        {
            _context.Update(project);
            await _context.SaveChangesAsync();
        }
    }
}
