using System.ComponentModel.DataAnnotations;

namespace IdentityDemo.DTOs
{
    public class UserCredentials
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
