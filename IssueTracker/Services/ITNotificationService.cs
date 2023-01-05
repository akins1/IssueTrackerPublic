using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services
{
    public class ITNotificationService : IITNotificationService
    {

        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IITRolesService _roleService;

        public ITNotificationService(ApplicationDbContext context, 
                                     IEmailSender emailSender, 
                                     IITRolesService roleService)
        {
            _context = context;
            _emailSender = emailSender;
            _roleService = roleService;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            try
            {
                await _context.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Notification>> GetReceivedNotificationsAsync(string userId)
        {
            try
            {
                List<Notification> notifications = await _context.Notifications
                    .Include(n => n.Recipient)
                    .Include(n => n.Sender)
                    .Include(n => n.Ticket)
                        .ThenInclude(t => t.Project)
                    .Where(n => n.RecipientId == userId)
                    .ToListAsync();

                return notifications;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Notification>> GetSentNotificationsAsync(string userId)
        {
            try
            {
                List<Notification> notifications = await _context.Notifications
                    .Include(n => n.Recipient)
                    .Include(n => n.Sender)
                    .Include(n => n.Ticket)
                        .ThenInclude(t => t.Project)
                    .Where(n => n.SenderId == userId)
                    .ToListAsync();

                return notifications;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject)
        {
            IssueTrackerUser recipient = await _context.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);

            if (recipient == null)
            {
                return false;
            }

            string recipientEmail = recipient.Email;
            string message = notification.Message;

            try
            {
                await _emailSender.SendEmailAsync(recipientEmail, emailSubject, message);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string roleName)
        {
            try
            {
                List<IssueTrackerUser> roleMembers = await _roleService.GetUsersInRoleAsync(roleName, companyId);

                foreach (IssueTrackerUser user in roleMembers)
                {
                    notification.RecipientId = user.Id;
                    await SendEmailNotificationAsync(notification, notification.Title);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task SendMembersEmailNotificationAsync(Notification notification, List<IssueTrackerUser> members)
        {
            try
            {
                foreach (IssueTrackerUser user in members)
                {
                    notification.RecipientId = user.Id;
                    await SendEmailNotificationAsync(notification, notification.Title);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
