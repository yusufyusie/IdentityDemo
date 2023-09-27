using IdentityDemo.Identity.Users;
using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.Models
{
    public class ApplicationUserLogin : IdentityUserLogin<string>
    {
        public virtual ApplicationUser User { get; set; }
    }
}
