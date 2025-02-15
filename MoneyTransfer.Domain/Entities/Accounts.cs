using System.ComponentModel.DataAnnotations;

namespace MoneyTransfer.Domain.Entities;

public partial class Accounts
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int AccountNumber { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public decimal Balance { get; set; }

    public DateTime? LastUpdatedBy { get; set; }

    public virtual Users User { get; set; } = null!;
}
