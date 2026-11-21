
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }

    public class RegisterViewModel
    {
        [Required]
        public string FullName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required]
        [Compare(nameof(Password),
            ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";

        [Required]
        public string Role { get; set; } = "Participant";
    }

    public class AuthResponse
    {
        public string Message { get; set; } = "";
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
