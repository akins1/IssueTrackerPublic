using IssueTracker.Models;

namespace IssueTracker.Services.Interfaces
{
    public interface IITTicketHistoryService
    {
        public Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId);

        public Task AddHistoryAsync(int ticketId, string model, string userId);

        public Task<List<TicketHistory>> GetProjectTicketHistoryAsync(int projectId, int companyId);

        Task<List<TicketHistory>> GetCompanyTicketHistoryAsync(int companyId);
    }
}
