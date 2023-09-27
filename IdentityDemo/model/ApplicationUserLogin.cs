using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.model
{
    public class ApplicationUserLogin : IdentityUserLogin<string>
    {
        public virtual ApplicationUser User { get; set; }
    }
}
