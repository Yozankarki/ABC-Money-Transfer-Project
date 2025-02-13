using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MoneyTransfer.Application.Interfaces;
using MoneyTransfer.Shared.DTOs;
using System.Security.Claims;

namespace MoneyTransfer.Web.Controllers
{
    public class AuthController : Controller
    {
        #region Imports
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        #endregion

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterDto()
            {
                FirstName = "",
                MiddleName = "",
                LastName = "",
                Email = "",
                Password = "",
                ConfirmPassword = ""
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.RegisterAsync(model);
            if (result.Succeeded)
            {
                return RedirectToAction("Login", "Auth");
            }
            return BadRequest(result.Errors);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new LoginDto()
            {
                Email = "",
                Password = "",
                RememberMe = false
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model, string? returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var response = await _authService.LoginAsync(model);
                if (response == null) 
                    return LocalRedirect(returnUrl!);
                else 
                    return RedirectToAction(nameof(Index), "Home");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized("User not found.");

                await _authService.LogoutAsync();

                // Clear the authentication cookie
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
