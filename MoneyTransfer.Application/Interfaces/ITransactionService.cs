using MoneyTransfer.Domain.Entities;
using MoneyTransfer.Shared.DTOs;

namespace MoneyTransfer.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<Result<AccountDto>> GetNewAccountDetailsAsync(string userId);
        Task<Result<AccountDto>> GetUserAccountInfo(string userId);
        Task<Result<ExchangeRate>> GetAllExchangeRates();
        Task AddAccountAsync(AccountDto accountDto);
        Task<IEnumerable<Accounts>> GetAllAccountsAsync();
        Task<Result<TransactionRequestDto>> ProcessTransaction(string userId);
        Task<Result<TransactionRequestDto>> SendTransaction(TransactionRequestDto model);
        Task<Result<AccountDto>> UpdateUserBalanceAsync(string userId, decimal amount);
        Task<Result<Transaction>> AddTransaction(Transaction model);

    }
}
