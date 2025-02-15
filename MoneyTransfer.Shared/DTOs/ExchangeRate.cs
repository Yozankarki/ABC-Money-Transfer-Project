namespace MoneyTransfer.Shared.DTOs
{
    public class ExchangeRate
    {
        public string? Date { get; set; }
        public string? PublishedOn { get; set; }
        public string? ModifiedOn { get; set; }
        public List<Rate>? Rates { get; set; }
    }

    public class Rate
    {
        public Currency? Currency { get; set; }
        public string? Buy { get; set; }
        public string? Sell { get; set; }
    }

    public class Currency
    {
        public string? Iso3 { get; set; }
        public string? Name { get; set; }
        public int? Unit { get; set; }
    }
}
