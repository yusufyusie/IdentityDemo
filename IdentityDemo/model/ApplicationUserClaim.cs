using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.model
{
    public class ApplicationUserClaim : IdentityUserClaim<string>
    {
        public virtual ApplicationUser User { get; set; }
    }
}
