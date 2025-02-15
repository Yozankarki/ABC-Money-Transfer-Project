using System.ComponentModel.DataAnnotations;

namespace MoneyTransfer.Domain.Entities;

public partial class ExchangeRates
{
    [Key]
    public int Id { get; set; }

    public string FromCurrency { get; set; } = null!;

    public string ToCurrency { get; set; } = null!;

    public decimal ExchangeRate { get; set; }

    public DateTime? LastUpdated { get; set; }
}
