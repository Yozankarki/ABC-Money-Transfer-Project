namespace MoneyTransfer.Shared.DTOs
{
    public class DashBoardDto
    {
        public int TotalUsers { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmountTransferred { get; set; }
        public List<UserDashBoradDto> Users { get; set; } = new List<UserDashBoradDto>();
    }

    public class UserDashBoradDto
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public List<Rate> Rates { get; set; } = new List<Rate>();
        public AccountDto Account { get; set; } = new AccountDto();
    }
}
