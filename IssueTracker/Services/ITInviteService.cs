using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.ComponentModel.Design;

namespace IssueTracker.Services
{
    public class ITInviteService : IITInviteService
    {
        private readonly ApplicationDbContext _context;

        public ITInviteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId)
        {
            Invite invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

            if (invite == null)
            {
                return false;
            }

            try
            {
                invite.isValid = false;
                invite.InviteeId = userId;
                
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddNewInviteAsync(Invite invite)
        {
            try
            {
                await _context.AddAsync(invite);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AnyInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                return await _context.Invites
                    .Where(i => i.CompanyId == companyId)
                    .AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Invite> GetInviteAsync(int inviteId, int companyId)
        {
            try
            {
                Invite invite = await _context.Invites
                        .Where(i => i.CompanyId == companyId)
                        .Include(i => i.Company)
                        .Include(i => i.Project)
                        .Include(i => i.Inviter)
                        .FirstOrDefaultAsync(i => i.Id == inviteId);

                return invite;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Invite> GetInviteAsync(Guid token, string email, int companyId)
        {
            try
            {
                Invite invite = await _context.Invites
                        .Where(i => i.CompanyId == companyId)
                        .Include(i => i.Company)
                        .Include(i => i.Project)
                        .Include(i => i.Inviter)
                        .FirstOrDefaultAsync(i => i.CompanyToken == token && i.InviteeEmail == email);

                return invite;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            if (token == null)
            {
                return false;
            }

            Invite invite = await _context.Invites
                .FirstOrDefaultAsync(i => i.CompanyToken == token);

            if (invite == null)
            {
                return false;
            }

            DateTime inviteDate = invite.InviteDate.DateTime;

            bool validDate = (DateTime.Now - inviteDate).TotalDays <= 7;

            if (validDate)
            {
                return invite.isValid;
            }

            return false;
        }
    }
}
