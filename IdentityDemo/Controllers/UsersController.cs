using AutoMapper;
using IdentityDemo.DTOs;
using IdentityDemo.Identity.Roles;
using IdentityDemo.Identity.Users;
using IdentityDemo.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;

namespace IdentityDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersionNeutral]
    public class UsersController : ControllerBase
    {
        private readonly IdentityDemoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper mapper;
        public UsersController(UserManager<ApplicationUser> userManager,
                            IdentityDemoDbContext dbContext,
                            IMapper mapper,
                            RoleManager<ApplicationRole> roleManager)
        {
            _context = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            this.mapper = mapper;
        }
        [HttpGet]
        [OpenApiOperation("Get list of all users.", "")]
        public async Task<ActionResult<List<UserDTO>>> GetListUsers()
        {
            var queryable = _context.Users.AsQueryable();
            var users = await queryable.OrderBy(x => x.UserName).ToListAsync();
            return mapper.Map<List<UserDTO>>(users);
        }
        [HttpGet("{id}/roles")]
        [OpenApiOperation("Get a user's roles.", "")]
        public async Task<List<UserRoleDto>> GetRolesAsync(string id)
        {
            var userRoles = new List<UserRoleDto>();

            var user = await _userManager.FindByIdAsync(id);
            var roles = await _roleManager.Roles.AsNoTracking().ToListAsync();
            foreach (var role in roles)
            {
                userRoles.Add(new UserRoleDto
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    Description = role.Description,
                    Enabled = await _userManager.IsInRoleAsync(user, role.Name)
                });
            }

            return userRoles;
        }
        [HttpPost("{id}/roles")]
        [OpenApiOperation("Update a user's assigned roles.", "")]
        public async Task<string> AssignRolesAsync(string id, UserRolesRequest request)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            var user = await _userManager.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
            _ = user ?? throw new InvalidOperationException($"User with ID {id} cannot be found");
            // Check if the user is an admin for which the admin role is getting disabled
            if (await _userManager.IsInRoleAsync(user, IdentityRoles.Admin)
                && request.UserRoles.Any(a => !a.Enabled && a.RoleName == IdentityRoles.Admin))
            {
                // Get count of users in Admin Role
                int adminCount = (await _userManager.GetUsersInRoleAsync(IdentityRoles.Admin)).Count;
            }
            foreach (var userRole in request.UserRoles)
            {
                // Check if Role Exists
                if (await _roleManager.FindByNameAsync(userRole.RoleName) is not null)
                {
                    if (userRole.Enabled)
                    {
                        if (!await _userManager.IsInRoleAsync(user, userRole.RoleName))
                        {
                            await _userManager.AddToRoleAsync(user, userRole.RoleName);
                        }
                    }
                    else
                    {
                        await _userManager.RemoveFromRoleAsync(user, userRole.RoleName);
                    }
                }
            }
            await _userManager.UpdateAsync(user);
            return "Success";

        }

        [HttpPost]
        [OpenApiOperation("Creates a new user.", "")]
        public async Task<ActionResult<string>> CreateAsync([FromForm] UserForRegistrationDto userRegistrationDto)
        {
            var user = mapper.Map<ApplicationUser>(userRegistrationDto);
            user.Id = Guid.NewGuid().ToString();
            var result = await _userManager.CreateAsync(user, userRegistrationDto.Password);
            if (result.Succeeded)
            {
                return user.Id;
            }
            else
            {
                throw new Exception("User creation failed! " + result.Errors.Select(x => x.Description).Aggregate((a, b) => a + ", " + b));
            }
        }
        [HttpPost("self-register")]
        [OpenApiOperation("Anonymous user creates a user.", "")]
        public async Task<string> SelfRegisterAsync([FromForm] UserForRegistrationDto userRegistrationDto)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = userRegistrationDto.UserName,
                FirstName = userRegistrationDto.FirstName,
                LastName = userRegistrationDto.LastName,
                Gender = userRegistrationDto.Gender,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(user, userRegistrationDto.Password);
            if (!result.Succeeded)
            {
                throw new Exception("User creation failed! " + result.Errors.Select(x => x.Description).Aggregate((a, b) => a + ", " + b));
            }
            await _userManager.AddToRoleAsync(user, IdentityRoles.Basic);
            var messages = new List<string>();
            return string.Join(" ", messages);

        }
    }
}