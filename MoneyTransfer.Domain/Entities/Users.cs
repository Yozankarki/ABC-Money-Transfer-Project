
using Microsoft.AspNetCore.Identity;

namespace MoneyTransfer.Domain.Entities
{
    public class Users: IdentityUser
    {
        public required string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public required string LastName { get; set; }
        public string? Address { get; set; } = "";
        public string? Country { get; set; } = "";
    }
}
