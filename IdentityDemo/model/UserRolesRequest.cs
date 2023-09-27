using IdentityDemo.DTOs;

namespace IdentityDemo.model
{
    public class UserRolesRequest
    {
        public List<UserRoleDto> UserRoles { get; set; } = new();
    }
}
