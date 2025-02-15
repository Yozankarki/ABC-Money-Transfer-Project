namespace MoneyTransfer.Shared.DTOs
{
    public class TransactionListDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public decimal TransferAmount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public string TransactionType { get; set; }
    }
}
