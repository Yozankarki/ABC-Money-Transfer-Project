using System.ComponentModel.DataAnnotations;

namespace MoneyTransfer.Shared.DTOs
{
    public class AccountDto
    {
        [Required]
        public string UserId { get; set; } = null!;

        public string UserName { get; set; } = "";

        [Required]
        public int AccountNumber { get; set; }

        [Required]
        public string CurrencyCode { get; set; } = "NPR";

        [Required]
        public decimal Balance { get; set; }

        public DateTime? LastUpdatedBy { get; set; }
    }
}
