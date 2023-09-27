using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.model
{
    public class ApplicationRoleClaim : IdentityRoleClaim<string>
    {
        public virtual ApplicationRole Role { get; set; }
    }
}
