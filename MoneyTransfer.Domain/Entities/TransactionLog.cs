using System.ComponentModel.DataAnnotations;

namespace MoneyTransfer.Domain.Entities;

public partial class TransactionLog
{
    [Key]
    public int Id { get; set; }

    public int TransactionId { get; set; }

    public string Status { get; set; } = null!;

    public string? Message { get; set; }

    public DateTime? LoggedAt { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
