using IdentityDemo.Identity.Users;
using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.Models
{
    public class ApplicationUserClaim : IdentityUserClaim<string>
    {
        public virtual ApplicationUser User { get; set; }
    }
}
