using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services
{
    public class ITCompanyInfoService : IITCompanyInfoService
    {
        private readonly ApplicationDbContext _context;

        public ITCompanyInfoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<IssueTrackerUser>> GetAllMembersAsync(int companyId)
        {
            return await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
        }

        public async Task<List<Project>> GetAllProjectsAsync(int companyId)
        {
            return await _context.Projects.Where(p => p.CompanyId == companyId)
                                          .Include(p => p.ProjectPriority)
                                          .Include(p => p.Members)
                                          .Include(p => p.Company)
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

        public async Task<List<Ticket>> GetAllTicketsAsync(int companyId)
        {
            List<Project> projects = await GetAllProjectsAsync(companyId);
            return projects.SelectMany(p => p.Tickets).ToList();

        }

        public async Task<Company> GetCompanyInfoByIdAsync(int? companyId)
        {
            if (companyId == null)
            {
                return new Company();
            }
            
            return await _context.Companies.Include(c => c.Members)
                                           .Include(c => c.Projects)
                                           .Include(c => c.Invites)
                                           .FirstOrDefaultAsync(c => c.Id == companyId);
        }
    }
}
