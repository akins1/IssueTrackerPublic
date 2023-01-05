using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services
{
    public class ITTicketHistoryService : IITTicketHistoryService
    {
        private readonly ApplicationDbContext _context;

        public ITTicketHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            // Ticket is New
            if (oldTicket == null && newTicket != null)
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket.Id,
                    Property = "",
                    PreviousValue = "",
                    CurrentValue = "",
                    CreatedOn = DateTimeOffset.Now,
                    UserId = userId,
                    Description = "New Ticket Created"
                };

                try
                {
                    await _context.AddAsync(history);
                    await _context.SaveChangesAsync();

                }
                
                catch (Exception)
                {

                    throw;
                }
            }
            else
            {
                // TITLE
                if (oldTicket.Title != newTicket.Title)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Title",
                        PreviousValue = oldTicket.Title,
                        CurrentValue = newTicket.Title,
                        CreatedOn = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket title: {newTicket.Title}"
                    };

                    await _context.AddAsync(history);
                }

                // DESCRIPTION
                if (oldTicket.Description != newTicket.Description)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Description",
                        PreviousValue = oldTicket.Description,
                        CurrentValue = newTicket.Description,
                        CreatedOn = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket description: {newTicket.Description}"
                    };

                    await _context.AddAsync(history);
                }

                // TYPE
                if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Type",
                        PreviousValue = oldTicket.TicketType.Name,
                        CurrentValue = newTicket.TicketType.Name,
                        CreatedOn = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket type: {newTicket.TicketType.Name}"
                    };

                    await _context.AddAsync(history);
                }

                // PRIORITY
                if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Priority",
                        PreviousValue = oldTicket.TicketPriority.Name,
                        CurrentValue = newTicket.TicketPriority.Name,
                        CreatedOn = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket priority: {newTicket.TicketPriority.Name}"
                    };

                    await _context.AddAsync(history);
                }

                // STATUS
                if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Status",
                        PreviousValue = oldTicket.TicketStatus.Name,
                        CurrentValue = newTicket.TicketStatus.Name,
                        CreatedOn = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket status: {newTicket.TicketStatus.Name}"
                    };

                    await _context.AddAsync(history);
                }

                // DEVELOPER
                if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Developer",
                        PreviousValue = oldTicket.DeveloperUser?.FullName ?? "Not Assigned",
                        CurrentValue = newTicket.DeveloperUser?.FullName,
                        CreatedOn = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket developer: {newTicket.DeveloperUser.FullName}"
                    };

                    await _context.AddAsync(history);
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public async Task AddHistoryAsync(int ticketId, string model, string userId)
        {
            try
            {
                Ticket ticket = await _context.Tickets.FindAsync(ticketId);
                string description = model.ToLower().Replace("ticket", "");

                description = $"New {description} added to Ticket: {ticket.Title}";

                TicketHistory history = new()
                {
                    TicketId = ticket.Id,
                    Property = model,
                    PreviousValue = "",
                    CurrentValue = "",
                    CreatedOn = DateTimeOffset.Now,
                    UserId = userId,
                    Description = description,
                };

                await _context.TicketHistories.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<List<TicketHistory>> GetCompanyTicketHistoryAsync(int companyId)
        {
            try
            {
                List<Project> projects = (await _context.Companies
                    .Include(c => c.Projects)
                        .ThenInclude(p => p.Tickets)
                            .ThenInclude(t => t.History)
                                .ThenInclude(h => h.User)
                    .FirstOrDefaultAsync(c => c.Id == companyId))
                    .Projects
                    .ToList();

                List<Ticket> tickets = projects.SelectMany(p => p.Tickets).ToList();

                List<TicketHistory> history = tickets.SelectMany(t => t.History).ToList();

                return history;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketHistory>> GetProjectTicketHistoryAsync(int projectId, int companyId)
        {
            try
            {
                Project project = await _context.Projects
                        .Where(p => p.CompanyId == companyId)
                        .Include(p => p.Tickets)
                            .ThenInclude(t => t.History)
                                .ThenInclude(u => u.User)
                        .FirstOrDefaultAsync(p => p.Id == projectId);

                List<TicketHistory> history = project.Tickets.SelectMany(t => t.History).ToList();

                return history;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
