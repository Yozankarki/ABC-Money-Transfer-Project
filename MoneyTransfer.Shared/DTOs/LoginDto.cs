using System.ComponentModel.DataAnnotations;

namespace MoneyTransfer.Shared.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; init; }

        [Required(ErrorMessage = "Password is required.")]
        public required string Password { get; init; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
