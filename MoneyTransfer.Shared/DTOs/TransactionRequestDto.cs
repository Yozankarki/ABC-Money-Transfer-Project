namespace MoneyTransfer.Shared.DTOs
{
    public class TransactionRequestDto
    {
        public int Id { get; set; }
        public string? SenderId { get; set; }
        public decimal SenderBalance { get; set; }
        public int ReceiverAccountNumber { get; set; }
        public int TransferAmount { get; set; }
        public decimal ExchangeRate  { get; set; }
        public string? SenderCurrencyCode { get; set; }
        public decimal PayoutAmount { get; set; }
        public string BankName { get; set; } = "ABC";
        public List<int> RecipientAccountNumbers { get; set; } = new List<int>();
        public List<string> RecipientNames { get; set; } = new List<string>();
    }
}
