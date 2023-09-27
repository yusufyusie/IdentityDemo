using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.model
{
    public class ApplicationUserToken : IdentityUserToken<string>
    {
        public virtual ApplicationUser User { get; set; }
    }
}
