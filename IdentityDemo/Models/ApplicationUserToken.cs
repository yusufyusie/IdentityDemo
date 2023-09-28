using IdentityDemo.Identity.Users;
using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.Models
{
    public class ApplicationUserToken : IdentityUserToken<string>
    {
        public virtual ApplicationUser User { get; set; }
    }
}
