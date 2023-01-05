using IssueTracker.Models;

namespace IssueTracker.Services.Interfaces
{
    public interface IITNotificationService
    {
        public Task AddNotificationAsync(Notification notification);

        public Task<List<Notification>> GetReceivedNotificationsAsync(string userId);

        public Task<List<Notification>> GetSentNotificationsAsync(string userId);

        public Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string roleName);

        public Task SendMembersEmailNotificationAsync(Notification notification, List<IssueTrackerUser> members);

        public Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject);
    }
}
