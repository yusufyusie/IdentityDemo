using System.ComponentModel.DataAnnotations;

namespace IdentityDemo.DTOs
{
    public class UserForRegistrationDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Required(ErrorMessage = "User name is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
