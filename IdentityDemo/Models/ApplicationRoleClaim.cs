using IdentityDemo.Identity.Roles;
using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.Models
{
    public class ApplicationRoleClaim : IdentityRoleClaim<string>
    {
        public virtual ApplicationRole Role { get; set; }
    }
}
