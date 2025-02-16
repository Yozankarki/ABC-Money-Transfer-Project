using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MoneyTransfer.Application.Interfaces;
using MoneyTransfer.Domain.Entities;
using MoneyTransfer.Shared.DTOs;
using Newtonsoft.Json;
using System.Reflection;

namespace MoneyTransfer.Application.Services
{
    public class TransactionService : ITransactionService
    {
        #region Imports 
        private readonly IRepoService<Accounts> _accountService;
        private readonly IRepoService<Transaction> _transactionService;
        private readonly IRepoService<TransactionLog> _transactionLogService;
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly HttpClient _httpClient;

        public TransactionService(IRepoService<Accounts> accountService, UserManager<Users> userManager, SignInManager<Users> signInManager, HttpClient httpClient, IRepoService<Transaction> transactionService, IRepoService<TransactionLog> transactionLogService)
        {
            _accountService = accountService;
            _userManager = userManager;
            _signInManager = signInManager;
            _httpClient = httpClient;
            _transactionService = transactionService;
            _transactionLogService = transactionLogService;
        }
        #endregion

        // Generate a unique 5-digit account number
        private int GenerateUniqueAccountNumber()
        {
            Random random = new Random();
            return random.Next(10000, 99999);
        }

        public async Task<Result<AccountDto>> GetNewAccountDetailsAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Result<AccountDto>.Failure("User not found.");
                }
                var account = await _accountService.GetAll().Where(x => x.UserId == userId).FirstOrDefaultAsync();

                if (account != null)
                {
                    return Result<AccountDto>.Failure("Account already exists.");
                }

                var accountDto = new AccountDto
                {
                    UserId = user.Id,
                    UserName = $"{user.FirstName} {user.MiddleName} {user.LastName}",
                    AccountNumber = GenerateUniqueAccountNumber(),
                    CurrencyCode = "NPR",
                    Balance = 0,
                    LastUpdatedBy = DateTime.UtcNow,
                };

                return Result<AccountDto>.Success(accountDto);
            }
            catch (Exception ex)
            {
                return Result<AccountDto>.Failure($"An error occurred while getting account details: {ex.Message}");
            }
        }

        public async Task<Result<AccountDto>> GetUserAccountInfo(string userId)
        {
            try
            {
                var accounts = await _accountService.GetAll().Include(a => a.User).FirstOrDefaultAsync(a => a.UserId == userId);

                if (accounts == null)
                {
                    return Result<AccountDto>.Success(new AccountDto
                    {
                        UserId = userId,
                        UserName = string.Empty,
                        AccountNumber = 0,
                        CurrencyCode = string.Empty,
                        Balance = 0,
                        LastUpdatedBy = DateTime.UtcNow
                    });
                }

                var accountDto = new AccountDto
                {
                    UserId = accounts.UserId,
                    UserName = $"{accounts.User.FirstName} {accounts.User.MiddleName} {accounts.User.LastName}",
                    AccountNumber = accounts.AccountNumber,
                    CurrencyCode = accounts.CurrencyCode,
                    Balance = accounts.Balance,
                    LastUpdatedBy = accounts.LastUpdatedBy
                };

                return Result<AccountDto>.Success(accountDto);
            }
            catch (Exception ex)
            {
                return Result<AccountDto>.Failure($"An error occurred while getting account details: {ex.Message}");
            }
        }

        public async Task<Result<ExchangeRate>> GetAllExchangeRates()
        {
            try
            {
                var apiUrl = "https://www.nrb.org.np/api/forex/v1/rates?page=1&per_page=5&from=2024-06-12&to=2024-06-12";
                var response = await _httpClient.GetStringAsync(apiUrl);
                var forexResponse = JsonConvert.DeserializeObject<ForexResponse>(response);

                var payload = forexResponse?.Data?.Payload?.FirstOrDefault();
                if (payload == null)
                {
                    return Result<ExchangeRate>.Failure("No exchange rate data available.");
                }

                var exchangeRate = new ExchangeRate
                {
                    Date = payload.Date,
                    PublishedOn = payload.PublishedOn,
                    ModifiedOn = payload.ModifiedOn,
                    Rates = payload.Rates ?? new List<Rate>()
                };

                return Result<ExchangeRate>.Success(exchangeRate);
            }
            catch (Exception ex)
            {
                return Result<ExchangeRate>.Failure($"No exchange rate data available for MYR.{ex.Message}");
            }
        }

        public async Task AddAccountAsync(AccountDto accountDto)
        {
            var newAccount = new Accounts
            {
                UserId = accountDto.UserId,
                AccountNumber = accountDto.AccountNumber,
                CurrencyCode = accountDto.CurrencyCode,
                Balance = accountDto.Balance,
                LastUpdatedBy = DateTime.UtcNow
            };
            await _accountService.AddAsync(newAccount);
        }

        public async Task<IEnumerable<Accounts>> GetAllAccountsAsync()
        {
            return await _accountService.GetAllAsync();
        }

        public async Task<Result<TransactionRequestDto>> ProcessTransaction(string userId)
        {
            try
            {
                var account = await _accountService.GetAll().Include(a => a.User).ToListAsync();
                if (account == null)
                {
                    return Result<TransactionRequestDto>.Failure("Account not found.");
                }
                var senderDetail = account.Where(x => x.UserId == userId).FirstOrDefault();
                if (senderDetail == null) return Result<TransactionRequestDto>.Failure("Cannot find sender Details.");

                var senderId = senderDetail.UserId;
                var senderAccountNo = senderDetail.AccountNumber;
                var senderCurrencyCode = senderDetail.CurrencyCode;
                var senderBalance = senderDetail.Balance;
                if (senderBalance == 0) return Result<TransactionRequestDto>.Failure("Insufficient balance.");

                var receiverDetails = account.Where(x => x.AccountNumber != senderAccountNo && x.CurrencyCode == "NPR");
                int[] receiverAccountNos = receiverDetails.Select(x => x.AccountNumber).ToArray();
                List<int> recipientAccountNumbers = receiverAccountNos.ToList();


                string[] receiverNames = receiverDetails.Select(x => x.User.FirstName + " " + x.User.MiddleName + " " + x.User.LastName).ToArray();
                List<string> receiverNamesList = receiverNames.ToList();

                if (senderCurrencyCode == null || senderCurrencyCode == "NPR")
                {
                    return Result<TransactionRequestDto>.Failure("User cannot send Funds");
                }

                var exchangeRateResult = await GetAllExchangeRates();
                if (!exchangeRateResult.Succeeded)
                {
                    return Result<TransactionRequestDto>.Failure("Cannot get exchange rate.");
                }

                // Filter the rates where Iso3 is "MYR"
                var filteredRate = exchangeRateResult.Data.Rates!
                    .Where(rate => rate.Currency?.Iso3 == "MYR")
                    .FirstOrDefault();

                if (filteredRate == null)
                {
                    return Result<TransactionRequestDto>.Failure("No exchange rate data available for MYR.");
                }

                var sellValue = filteredRate.Sell;

                var transaction = new TransactionRequestDto
                {
                    SenderId = senderId,
                    SenderBalance = senderBalance,
                    ReceiverAccountNumber = 0,
                    TransferAmount = 0,
                    ExchangeRate = decimal.Parse(sellValue!),
                    SenderCurrencyCode = "MYR",
                    PayoutAmount = 0,
                    BankName = "",
                    RecipientAccountNumbers = recipientAccountNumbers,
                    RecipientNames = receiverNamesList
                };
                return Result<TransactionRequestDto>.Success(transaction);
            }
            catch (Exception ex)
            {
                return Result<TransactionRequestDto>.Failure($"An error occurred while processing transaction: {ex.Message}");
            }
        }

        public async Task<Result<TransactionRequestDto>> SendTransaction(TransactionRequestDto model)
        {
            var strategy = await _accountService.BeginExecutionAsync();

            Result<TransactionRequestDto> result = Result<TransactionRequestDto>.Failure("Unknown error occurred.");

            await strategy.ExecuteAsync(async () =>
            {
                await using (var transaction = await _accountService.BeginTransactionAsync())
                {
                    try
                    {
                        var account = await _accountService.GetAll().Include(a => a.User).ToListAsync();

                        if (account == null || !account.Any())
                        {
                            await transaction.RollbackAsync();
                            return Result<TransactionRequestDto>.Failure("No accounts found.");
                        }

                        var senderDetails = account.FirstOrDefault(x => x.UserId == model.SenderId);
                        var receiverDetails = account.FirstOrDefault(x => x.AccountNumber == model.ReceiverAccountNumber);

                        if (senderDetails == null || receiverDetails == null)
                        {
                            await transaction.RollbackAsync();
                            return Result<TransactionRequestDto>.Failure("Sender or receiver account not found.");
                        }

                        var senderId = senderDetails.UserId;
                        var receiverAccountNo = receiverDetails.AccountNumber;
                        var receiverId = receiverDetails.UserId;

                        if (senderId == null || receiverId == null)
                        {
                            await transaction.RollbackAsync();
                            return Result<TransactionRequestDto>.Failure("Sender or receiver ID not found.");
                        }

                        var senderTotalBalance = senderDetails.Balance;
                        var receiverTotalBalance = receiverDetails.Balance;

                        if (senderTotalBalance == 0 || senderTotalBalance < model.TransferAmount)
                        {
                            await transaction.RollbackAsync();
                            return Result<TransactionRequestDto>.Failure("Insufficient balance.");
                        }

                        var senderCurrencyCode = senderDetails.CurrencyCode;
                        var receiverCurrencyCode = receiverDetails.CurrencyCode;

                        if (receiverCurrencyCode != "NPR")
                        {
                            await transaction.RollbackAsync();
                            return Result<TransactionRequestDto>.Failure("User cannot send funds to this currency.");
                        }

                        var convertedAmount = model.ExchangeRate * model.TransferAmount;
                        if (model.PayoutAmount > convertedAmount || convertedAmount != model.PayoutAmount)
                        {
                            await transaction.RollbackAsync();
                            return Result<TransactionRequestDto>.Failure("Insufficient balance or invalid payout amount.");
                        }

                        // Create Transaction Entry
                        var newTransaction = new Transaction
                        {
                            SenderId = senderId.ToString(),
                            ReceiverId = receiverId.ToString(),
                            TransferAmount = model.TransferAmount,
                            CurrencyCode = "NPR",
                            ExchangeRate = model.ExchangeRate,
                            ConvertedAmount = convertedAmount,
                            TransactionType = "Transfer",
                            TransactionStatus = "Completed",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _transactionService.AddAsync(newTransaction);
                        model.Id = newTransaction.Id;

                        // Update Account Balance after transaction
                        // Debit Sender Account
                        senderDetails.Balance -= model.TransferAmount;
                        await UpdateUserBalanceAsync(senderId.ToString(), senderDetails.Balance);

                        // Credit Receiver Account
                        receiverDetails.Balance += convertedAmount;
                        await UpdateUserBalanceAsync(receiverId.ToString(), receiverDetails.Balance);

                        // Commit Transaction
                        await transaction.CommitAsync();

                        // Log Successful Transaction
                        await LogTransaction(model, "Success", "Transaction completed successfully.");
                        return Result<TransactionRequestDto>.Success("Transaction sent successfully.");
                    }
                    catch (Exception ex)
                    {
                        await LogTransaction(model, "Failed", $"An error occurred: {ex.Message}");
                        await transaction.RollbackAsync();
                        return Result<TransactionRequestDto>.Failure($"An error occurred while sending transaction: {ex.Message}");
                    }
                }
            });
            return result;
        }

        private async Task LogTransaction(TransactionRequestDto model, string status, string message)
        {
            var logEntry = new TransactionLog
            {
                TransactionId = model.Id,
                Status = status,
                Message = message,
                LoggedAt = DateTime.UtcNow
            };
            await _transactionLogService.AddAsync(logEntry);
        }

        public async Task<Result<AccountDto>> UpdateUserBalanceAsync(string userId, decimal amount)
        {
            try
            {
                var account = await _accountService.GetByUserIdAsync(userId);
                if (account == null)
                {
                    return Result<AccountDto>.Failure("Account not found.");
                }

                account.Balance = amount;
                account.LastUpdatedBy = DateTime.UtcNow;

                await _accountService.UpdateAsync(account);

                return Result<AccountDto>.Success("User Balance Updated Successfully.");
            }
            catch (Exception ex)
            {
                return Result<AccountDto>.Failure($"An error occurred while updating account balance: {ex.Message}");
            }
        }

        public async Task<Result<Transaction>> AddTransaction(Transaction model)
        {
            using (var transaction = await _accountService.BeginTransactionAsync())
                try
                {
                    if (model == null) return Result<Transaction>.Failure("error");
                    await _transactionService.AddAsync(model);
                    return Result<Transaction>.Success("Transaction added Successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Result<Transaction>.Failure($"An error occurred while adding transaction: {ex.Message}");
                }
        }

        public async Task<List<TransactionListDto>> GetTransactionsAsync()
        {
            var transactions = await _transactionService.GetAll()
                .Include(t => t.Sender)
                .Include(t => t.Receiver)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var transactionDtos = transactions.Select(t => new TransactionListDto
            {
                Id = t.Id,
                SenderId = t.SenderId,
                CreatedAt = t.CreatedAt ?? DateTime.Now,
                SenderName = t.Sender.FirstName + t.Sender.MiddleName + t.Sender.LastName,
                ReceiverName = t.Receiver.FirstName + t.Receiver.MiddleName + t.Receiver.LastName,
                TransferAmount = t.TransferAmount,
                ConvertedAmount = t.ConvertedAmount,
                TransactionType = t.TransactionType
            }).ToList();

            return transactionDtos;
        }

        public async Task<TransactionListDto> GetTransactionByIdAsync(int id)
        {
            var transaction = await _transactionService.GetAll()
                .Include(t => t.Sender)
                .Include(t => t.Receiver)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return new TransactionListDto();
            }

            // Safely handle null Sender and Receiver
            string senderName = transaction.Sender != null
                ? $"{transaction.Sender.FirstName} {transaction.Sender.LastName}"
                : "Unknown Sender";

            string receiverName = transaction.Receiver != null
                ? $"{transaction.Receiver.FirstName} {transaction.Receiver.LastName}"
                : "Unknown Receiver";

            return new TransactionListDto
            {
                Id = transaction.Id,
                SenderId = transaction.SenderId,
                CreatedAt = transaction.CreatedAt ?? DateTime.Now,
                SenderName = senderName,
                ReceiverName = receiverName,
                TransferAmount = transaction.TransferAmount,
                ConvertedAmount = transaction.ConvertedAmount,
                TransactionType = transaction.TransactionType
            };
        }

    }
}
