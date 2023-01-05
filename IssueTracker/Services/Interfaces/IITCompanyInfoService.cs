using IssueTracker.Models;

namespace IssueTracker.Services.Interfaces
{
    public interface IITCompanyInfoService
    {
        public Task<Company> GetCompanyInfoByIdAsync(int? companyId);

        public Task<List<IssueTrackerUser>> GetAllMembersAsync(int companyId);

        public Task<List<Project>> GetAllProjectsAsync(int companyId);

        public Task<List<Ticket>> GetAllTicketsAsync(int companyId);


    }
}
