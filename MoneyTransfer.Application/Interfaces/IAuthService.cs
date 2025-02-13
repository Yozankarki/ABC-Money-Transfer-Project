using Microsoft.AspNetCore.Identity;
using MoneyTransfer.Domain.Entities;
using MoneyTransfer.Shared.DTOs;

namespace MoneyTransfer.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<string>> RegisterAsync(RegisterDto model);
        Task<LoginDto> LoginAsync(LoginDto model);
        Task LogoutAsync();
    }
}
