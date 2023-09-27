using AutoMapper;
using IdentityDemo.DTOs;
using IdentityDemo.Identity.Roles;
using IdentityDemo.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System.Security.Claims;

namespace IdentityDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersionNeutral]
    public class RolesController : ControllerBase
    {
        private readonly IdentityDemoDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper mapper;
        public RolesController(IdentityDemoDbContext dbContext, IMapper mapper, RoleManager<ApplicationRole> roleManager)
        {
            _context = dbContext;
            _roleManager = roleManager;
            this.mapper = mapper;
        }
        [HttpGet("listRoles")]
        [OpenApiOperation("Get a list of all roles.", "")]
        public async Task<ActionResult<List<RoleDto>>> GetListRoles()
        {
            var roles = await _context.Roles.ToListAsync();
            return mapper.Map<List<RoleDto>>(roles);
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
