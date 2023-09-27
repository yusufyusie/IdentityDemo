using AutoMapper;
using IdentityDemo.DTOs;
using IdentityDemo.model;
using IdentityDemo.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using NSwag.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IdentityDemoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        private readonly IStringLocalizer _t;
        public AccountsController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IdentityDemoDbContext dbContext,
            IConfiguration configuration,
            IMapper mapper,
            RoleManager<ApplicationRole> roleManager,
            IStringLocalizer t)
        {
            _context = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            this.configuration = configuration;
            this.mapper = mapper;
            _t = t;
        }
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromForm] UserCredentials userCredentials)
        {
            var result = await _signInManager.PasswordSignInAsync(userCredentials.UserName,
                userCredentials.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return await GenerateJwtToken(userCredentials);
            }
            else
            {
                return BadRequest("Incorrect Login");
            }
        }

        [HttpPost("register user")]
        [OpenApiOperation("Creates a new user.", "")]
        public async Task<ActionResult<AuthenticationResponse>> Register([FromForm] UserForRegistrationDto userRegistrationDto)
        {
            var user = mapper.Map<ApplicationUser>(userRegistrationDto);
            user.Id = Guid.NewGuid().ToString();
            var result = await _userManager.CreateAsync(user, userRegistrationDto.Password);
            if (result.Succeeded)
            {
                return StatusCode(201);
            }
            else
            {
                return BadRequest(result.Errors);
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
            var messages = new List<string> { string.Format(_t["User {0} Registered."], user.UserName) };
            return string.Join(" ", messages);


        }
        private async Task<AuthenticationResponse> GenerateJwtToken(UserCredentials userCredentials)
        {
            var clams = new List<Claim>()
           {
                new Claim("UserName",  userCredentials.UserName)
            };
            var user = await _userManager.FindByNameAsync(userCredentials.UserName);
            var claimDb = await _userManager.GetClaimsAsync(user);
            clams.AddRange(claimDb);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["MyKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.Now.AddDays(1);
            var token = new JwtSecurityToken(
                        issuer: null,
                        audience: null,
                         claims: clams,
                      expires: expiration,
                     signingCredentials: creds
                                                                                                          );
            return new AuthenticationResponse()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
        [HttpGet("listUsers")]
        [OpenApiOperation("Get list of all users.", "")]
        public async Task<ActionResult<List<UserDTO>>> GetListUsers()
        {
            var queryable = _context.Users.AsQueryable();
            var users = await queryable.OrderBy(x => x.UserName).ToListAsync();
            return mapper.Map<List<UserDTO>>(users);
        }
        [HttpPost("register or update role")]
        [OpenApiOperation("Create or update a role.", "")]
        public async Task<IActionResult> RegisterRoleAsync([FromForm] CreateOrUpdateRoleDto roleRegistrationDto)
        {
            if (string.IsNullOrEmpty(roleRegistrationDto.Id))
            {
                // Create a new role.
                var role = mapper.Map<ApplicationRole>(roleRegistrationDto);
                role.Id = Guid.NewGuid().ToString();
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.TryAddModelError(error.Code, error.Description);
                    }

                    return BadRequest(ModelState);
                }
                await _roleManager.AddClaimAsync(role, new Claim("Create Role", "true"));
                return StatusCode(201);
            }
            else
            {
                // Update an existing role.
                var role = await _roleManager.FindByIdAsync(roleRegistrationDto.Id);

                _ = role ?? throw new InvalidOperationException($"Role with ID {roleRegistrationDto.Id} cannot be found");
                if (IdentityRoles.IsDefault(role.Name))
                {
                    ModelState.AddModelError(string.Empty, "Cannot update a default role");
                    return BadRequest(ModelState);
                }
                role.Name = roleRegistrationDto.Name;
                role.Description = roleRegistrationDto.Description;
                role.NormalizedName = roleRegistrationDto.Name.ToUpperInvariant();
                var result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.TryAddModelError(error.Code, error.Description);
                    }

                    return BadRequest(ModelState);
                }
                await _roleManager.AddClaimAsync(role, new Claim("Update Role", "true"));
                return Ok(201);
            }
        }
        [HttpGet("listRoles")]
        [OpenApiOperation("Get a list of all roles.", "")]
        public async Task<ActionResult<List<RoleDto>>> GetListRoles()
        {
            var roles = await _context.Roles.ToListAsync();
            return mapper.Map<List<RoleDto>>(roles);
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
            _ = user ?? throw new InvalidOperationException(_t["User Not Found."]);
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
            return _t["User Roles Updated Successfully."];

        }

        [HttpPut("{id}/permissions")]
        [OpenApiOperation("Update a role's permissions.", "")]
        public async Task<ActionResult<string>> UpdatePermissionsAsync([FromForm] string id, UpdateRolePermissions permissions)
        {
            var role = await _roleManager.FindByIdAsync(id);
            _ = role ?? throw new InvalidOperationException($"Role with ID {id} cannot be found");
            if (IdentityRoles.IsDefault(role.Name))
            {
                ModelState.AddModelError(string.Empty, "Cannot update a default role");
                return BadRequest(ModelState);
            }
            var claims = await _roleManager.GetClaimsAsync(role);
            var claimValues = claims.Select(x => x.Value).ToList();
            var newClaims = permissions.Permissions.Except(claimValues);
            var removedClaims = claimValues.Except(permissions.Permissions);
            foreach (var newClaim in newClaims)
            {
                await _roleManager.AddClaimAsync(role, new Claim("Permission", newClaim));
            }
            foreach (var removedClaim in removedClaims)
            {
                var claim = claims.FirstOrDefault(x => x.Value == removedClaim);
                await _roleManager.RemoveClaimAsync(role, claim);
            }
            return Ok();
        }
    }
}