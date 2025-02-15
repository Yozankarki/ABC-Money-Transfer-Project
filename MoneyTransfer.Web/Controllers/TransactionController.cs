using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyTransfer.Application.Interfaces;
using MoneyTransfer.Shared.DTOs;
using System.Security.Claims;

namespace MoneyTransfer.Web.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        #region Imports
        private readonly ITransactionService _TransactionService;
        private readonly HttpClient _httpClient;
        public TransactionController(ITransactionService transactionService, HttpClient httpClient)
        {
            _TransactionService = transactionService;
            _httpClient = httpClient;
        }
        #endregion

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var accountInfoResult = await _TransactionService.GetUserAccountInfo(userId);
            var exchangeRateResult = await _TransactionService.GetAllExchangeRates();

            var dashboardInfo = new UserDashBoradDto
            {
                FullName = accountInfoResult.Data.UserName,
                Address = accountInfoResult.Data.CurrencyCode,
                Rates = exchangeRateResult.Data?.Rates ?? new List<Rate>(),
                Account = new AccountDto
                {
                    AccountNumber = accountInfoResult.Data.AccountNumber,
                    Balance = accountInfoResult.Data.Balance,
                    CurrencyCode = accountInfoResult.Data.CurrencyCode,
                    LastUpdatedBy = accountInfoResult.Data.LastUpdatedBy,
                    UserId = accountInfoResult.Data.UserId,
                    UserName = accountInfoResult.Data.UserName
                }
            };

            return View(dashboardInfo);
        }

        [HttpGet]
        public async Task<IActionResult> NewAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var result = await _TransactionService.GetNewAccountDetailsAsync(userId);
            if (result.Succeeded)
            {
                return View(result.Data);
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> NewAccount(AccountDto model)
        {
            if (!ModelState.IsValid)
                return View("NewAccount", model);

            await _TransactionService.AddAccountAsync(model);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Transfer()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            //var result;
            var processtransaction = await _TransactionService.ProcessTransaction(userId);
            if (processtransaction.Succeeded)
            {
                return View(processtransaction.Data);
            }
            else
            {
                ModelState.AddModelError(string.Empty, processtransaction.Message);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Transfer(TransactionRequestDto model)
        {
            if (!ModelState.IsValid)
                return View("Transfer", model);

            var sendTransaction = await _TransactionService.SendTransaction(model);
            if (sendTransaction.Succeeded)
            {
                return RedirectToAction("TransactionList");
            }
            else
            {
                ModelState.AddModelError(string.Empty, sendTransaction.Message);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult TransactionList()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Statement()
        {
            return View();
        }
        [HttpGet]
        public IActionResult UpdateProfile()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Logout()
        {
            return View();
        }
    }
}
