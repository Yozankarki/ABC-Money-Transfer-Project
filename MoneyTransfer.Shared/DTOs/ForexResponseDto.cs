namespace MoneyTransfer.Shared.DTOs
{
    public class ForexResponse
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public List<Payload> Payload { get; set; }
    }

    public class Payload
    {
        public string Date { get; set; }
        public string PublishedOn { get; set; }
        public string ModifiedOn { get; set; }
        public List<Rate> Rates
        {
            get; set;
        }
    }
}