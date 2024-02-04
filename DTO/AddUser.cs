using System.ComponentModel.DataAnnotations;

namespace Practiseproject.DTO
{
    public class AddUser
    {
        [Required(ErrorMessage = "Username is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(5, ErrorMessage = "The Password field must be at least 5 characters.")]
        public string? Password { get; set; }
        
   

    }
}
