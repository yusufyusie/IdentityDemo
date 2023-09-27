using IdentityDemo.DTOs;
using IdentityDemo.model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersionNeutral]
    public class TokensController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration configuration;
        public TokensController(UserManager<ApplicationUser> userManager,
                                SignInManager<ApplicationUser> signInManager,
                               IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this.configuration = configuration;
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
    }
}
