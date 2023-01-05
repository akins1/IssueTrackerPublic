using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Models.Enums;
using IssueTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;

namespace IssueTracker.Services
{
    public class ITTicketService : IITTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IITRolesService _rolesService;
        private readonly IITProjectService _projectService;

        public ITTicketService(ApplicationDbContext context, IITRolesService roleService, IITProjectService projectService)
        {
            _context = context;
            _rolesService = roleService;
            _projectService = projectService;
        }

        public async Task AddNewTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTicketCommentAsync(TicketComment comment)
        {
            try
            {
                await _context.AddAsync(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
            
        }


        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true;

                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RestoreTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = false;

                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            Ticket ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);

            try
            {
                if (ticket != null)
                {
                    try
                    {
                        ticket.DeveloperUserId = userId;

                        ticket.TicketStatusId = (await LookupTicketStatusIdAsync("Development")).Value;
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Projects
                    .Where(p => p.CompanyId == companyId)
                    .SelectMany(p => p.Tickets)
                        .Include(t => t.Attachments)
                        .Include(t => t.Comments)
                        .Include(t => t.History)
                        .Include(t => t.OwnerUser)
                        .Include(t => t.DeveloperUser)
                        .Include(t => t.TicketPriority)
                        .Include(t => t.TicketStatus)
                        .Include(t => t.TicketType)
                        .Include(t => t.Project)
                    .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            try
            {
                List<Ticket> tickets = await GetAllTicketsByCompanyAsync(companyId);
                
                return tickets.Where(t => t.TicketPriority.Name == priorityName).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            try
            {
                List<Ticket> tickets = await GetAllTicketsByCompanyAsync(companyId);

                return tickets.Where(t => t.TicketStatus.Name == statusName).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            try
            {
                List<Ticket> tickets = await GetAllTicketsByCompanyAsync(companyId);

                return tickets.Where(t => t.TicketType.Name == typeName).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await GetAllTicketsByCompanyAsync(companyId);

                return tickets.Where(t => t.Archived == true).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByPriorityAsync(companyId, priorityName))
                    .Where(t => t.ProjectId == projectId)
                    .ToList();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetTicketsByRoleAsync(role, userId, companyId))
                    .Where(t => t.ProjectId == projectId)
                    .ToList();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByStatusAsync(companyId, statusName))
                    
                    .Where(t => t.ProjectId == projectId)
                    .ToList();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            List<Ticket> tickets = new();

            try
            {
                tickets = (await GetAllTicketsByTypeAsync(companyId, typeName))
                    .Where(t => t.ProjectId == projectId)
                    .ToList();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.TicketType)
                    .Include(t => t.OwnerUser)
                    .Include(t => t.DeveloperUser)
                    .Include(t => t.Comments)
                    .Include(t => t.History)
                    .Include(t => t.Attachments)
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == ticketId);

            }
            catch (Exception)
            {

                throw;
            }        }

        public async Task<IssueTrackerUser> GetTicketDeveloperAsync(int ticketId, int companyId)
        {
            //IssueTrackerUser developer = new();

            try
            {
                Ticket ticket = (await GetAllTicketsByCompanyAsync(companyId)).FirstOrDefault(t => t.Id == ticketId);
                
                if (ticket?.DeveloperUserId != null)
                {
                    return ticket.DeveloperUser;
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId)
        {
            List<Ticket> tickets = new();

            try
            {
                
                if (role == RoleEnum.Admin.ToString())
                {
                    tickets = await GetAllTicketsByCompanyAsync(companyId);
                }
                else if (role == RoleEnum.ProjectManager.ToString())
                {
                    tickets = await GetTicketsByUserIdAsync(userId, companyId);
                }
                else if (role == RoleEnum.Developer.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId))
                        .Where(t => t.DeveloperUserId == userId)
                        .ToList();
                }
                else if (role == RoleEnum.Submitter.ToString())
                {
                    tickets = (await GetAllTicketsByCompanyAsync(companyId))
                        .Where(t => t.OwnerUserId == userId)
                        .ToList();
                }
                
            }
            catch (Exception)
            {

                throw;
            }

            return tickets;
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            IssueTrackerUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            List<Ticket> tickets = new();

            try
            {
                if (await _rolesService.IsUserInRoleAsync(user, RoleEnum.Admin.ToString()))
                {
                    tickets = (await _projectService
                        .GetAllProjectsByCompany(companyId))
                        .SelectMany(p => p.Tickets)
                        .ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(user, RoleEnum.ProjectManager.ToString()))
                {
                    tickets = (await _projectService
                        .GetUserProjectsAsync(userId))
                        .SelectMany(t => t.Tickets)
                        .ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(user, RoleEnum.Developer.ToString()))
                {
                    tickets = (await _projectService
                        .GetAllProjectsByCompany(companyId))
                        .SelectMany(p => p.Tickets)
                        .Where(t => t.DeveloperUserId == userId)
                        .ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(user, RoleEnum.Submitter.ToString()))
                {
                    tickets = (await _projectService
                        .GetAllProjectsByCompany(companyId))
                        .SelectMany(t => t.Tickets)
                        .Where(t => t.OwnerUserId == userId)
                        .ToList();
                }

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(Guid ticketAttachmentId)
        {
            try
            {
                TicketAttachment ticketAttachment = await _context.TicketAttachments
                        .Include(ta => ta.User)
                        .FirstOrDefaultAsync(ta => ta.Id == ticketAttachmentId);

                return ticketAttachment;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = (await GetAllTicketsByCompanyAsync(companyId)).Where(t => string.IsNullOrEmpty(t.DeveloperUserId)).ToList();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId)
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.DeveloperUser)
                    .Include(t => t.Project)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.TicketType)
                    .Include(t => t.Attachments)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == ticketId);
            }
            catch (Exception)
            {

                throw;
            }
        }



        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            try
            {
                TicketPriority priority = await _context.TicketPriorities.FirstOrDefaultAsync(tp => tp.Name == priorityName);
                
                return priority?.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            try
            {
                TicketStatus status = await _context.TicketStatuses.FirstOrDefaultAsync(ts => ts.Name == statusName);

                return status?.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            try
            {
                TicketType type = await _context.TicketTypes.FirstOrDefaultAsync(ts => ts.Name == typeName);

                return type?.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
