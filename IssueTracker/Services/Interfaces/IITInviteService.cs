using IssueTracker.Models;

namespace IssueTracker.Services.Interfaces
{
    public interface IITInviteService
    {
        public Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId);

        public Task AddNewInviteAsync(Invite invite);

        public Task<bool> AnyInviteAsync(Guid token, string email, int companyId);

        public Task<Invite> GetInviteAsync(int inviteId, int companyId);

        public Task<Invite> GetInviteAsync(Guid token, string email, int companyId);

        public Task<bool> ValidateInviteCodeAsync(Guid? token);
    }
}
