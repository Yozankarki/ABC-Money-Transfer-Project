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
