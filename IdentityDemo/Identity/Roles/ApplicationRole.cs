using IdentityDemo.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.Identity.Roles
{
    public class ApplicationRole : IdentityRole<string>
    {
        public string? Description { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
        public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; }

        public ApplicationRole(string name, string? description = null)
            : base(name)
        {
            Description = description;
            NormalizedName = name.ToUpperInvariant();
        }
    }
}
