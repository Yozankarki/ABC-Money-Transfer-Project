using System.ComponentModel.DataAnnotations;

namespace MoneyTransfer.Domain.Entities;
    
public partial class Transaction
{
    [Key]
    public int Id { get; set; }

    public string SenderId { get; set; } = null!;

    public string ReceiverId { get; set; } = null!;

    public decimal TransferAmount { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public decimal ExchangeRate { get; set; }

    public decimal ConvertedAmount { get; set; }

    public string TransactionType { get; set; } = null!;

    public string TransactionStatus { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Users Receiver { get; set; } = null!;

    public virtual Users Sender { get; set; } = null!;

    public virtual ICollection<TransactionLog> TransactionLogs { get; set; } = new List<TransactionLog>();
}
