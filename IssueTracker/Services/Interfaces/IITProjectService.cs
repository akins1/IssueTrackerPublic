using IssueTracker.Models;

namespace IssueTracker.Services.Interfaces
{
    public interface IITProjectService
    {
        public Task AddNewProjectAsync(Project project);

        public Task<bool> AddProjectManagerAsync(string userId, int projectId);

        public Task<bool> AddUserToProjectAsync(string userId, int projectId);

        public Task ArchiveProjectAsync(Project project);

        public Task<List<Project>> GetAllProjectsByCompany(int companyId);

        public Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName);

        public Task<List<IssueTrackerUser>> GetAllProjectMembersExceptPMAsync(int projectId);

        public Task<List<Project>> GetArchivedProjectsByCompany(int companyId);

        public Task<List<Project>> GetUnassignedProjectsByCompany(int companyId);

        public Task<List<IssueTrackerUser>> GetDevelopersOnProjectAsync(int projectId);

        public Task<IssueTrackerUser> GetProjectManagerAsync(int projectId);

        public Task<List<IssueTrackerUser>> GetProjectMembersByRoleAsync(int projectId, string roleName);

        public Task<Project> GetProjectByIdAsync(int projectId, int companyId);

        public Task<List<IssueTrackerUser>> GetSubmittersOnProjectAsync(int projectId);

        public Task<List<IssueTrackerUser>> GetUsersNotOnProjectAsync(int projectId, int companyId);

        public Task<IssueTrackerUser> GetUserAsync(string userId);

        public Task<List<Project>> GetUserProjectsAsync(string userId);

        public Task<bool> IsUserOnProjectAsync(string userId, int projectId);

        public Task<bool> IsUserProjectManagerAsync(string userId, int projectId);


        public Task<int> LookupProjectPriorityId(string priorityName);

        public Task RemoveProjectManagerAsync(int projectId);

        public Task RemoveUsersFromProjectByRoleAsync(string roleName, int projectId);

        public Task RemoveUserFromProjectAsync(string userId, int projectId);

        public Task RestoreProjectAsync(Project project);

        public Task UpdateProjectAsync(Project project);
    }
}
