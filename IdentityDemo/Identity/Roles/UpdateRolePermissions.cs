namespace IdentityDemo.Identity.Roles
{
    public class UpdateRolePermissions
    {
        public string RoleId { get; set; } = default!;
        public List<string> Permissions { get; set; } = default!;
    }
}
