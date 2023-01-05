using IssueTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace IssueTracker.Services.Factories
{
    public class ITUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IssueTrackerUser, IdentityRole>
    {
        public ITUserClaimsPrincipalFactory(UserManager<IssueTrackerUser> userManager,
                                            RoleManager<IdentityRole> roleManager,
                                            IOptions<IdentityOptions> optionsAccessor)
                                            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IssueTrackerUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
            
            return identity;
        }
    }
}
