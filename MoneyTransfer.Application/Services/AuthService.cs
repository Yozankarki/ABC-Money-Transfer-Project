using Microsoft.AspNetCore.Identity;
using MoneyTransfer.Application.Interfaces;
using MoneyTransfer.Domain.Entities;
using MoneyTransfer.Shared.DTOs;

namespace MoneyTransfer.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;

        public AuthService(UserManager<Users> userManager, SignInManager<Users> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<Result<string>> RegisterAsync(RegisterDto model)
        {
            try
            {
                var existingUser = await _userManager.FindByNameAsync(model.FirstName);
                var existingEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null) return Result<string>.Failure("User Name already exists.");
                if (existingEmail != null) return Result<string>.Failure("Email already registered.");

                var user = new Users {
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return Result<string>.Success("User registered successfully!");
                }
                else
                {
                    return Result<string>.Failure("Error occurred.");
                }
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(ex.Message);
            }
        }

        public async Task<LoginDto> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) throw new UnauthorizedAccessException("Invalid email or password.");

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
            if (!result.Succeeded)
                throw new UnauthorizedAccessException("Invalid email or password.");

            await _userManager.UpdateAsync(user);
            return model;
        }

        public async Task LogoutAsync()
        {
          await  _signInManager.SignOutAsync();
        }
    }
}
