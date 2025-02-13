
using System.ComponentModel.DataAnnotations;

namespace MoneyTransfer.Shared.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Enter First Name")]
        public required string FirstName { get; set; }

        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Enter Last Name")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("Password", ErrorMessage = "Confirm password doesn't match, Type again!")]
        public required string ConfirmPassword { get; set; }
    }
}
