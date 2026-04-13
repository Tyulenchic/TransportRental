using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportRental.Core.Services;
using TransportRental.Web.Models;

namespace TransportRental.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var user = await _authService.LoginAsync(model.Username, model.Password);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.Name),
            new Claim("UserId", user.Id.ToString()),
            new Claim("Username", user.Username)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [Authorize(Roles = "Администратор")]
    [HttpGet]
    public async Task<IActionResult> Register()
    {
        var users = await _authService.GetAllUsersAsync();
        ViewBag.Roles = await GetRolesFromUsersAsync();
        return View();
    }

    [Authorize(Roles = "Администратор")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = await GetRolesFromUsersAsync();
            return View(model);
        }

        try
        {
            await _authService.RegisterAsync(model.Username, model.Password, model.FullName, model.RoleId);
            TempData["Success"] = "Пользователь успешно создан";
            return RedirectToAction("Index", "Admin");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Roles = await GetRolesFromUsersAsync();
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    private async Task<List<(int Id, string Name)>> GetRolesFromUsersAsync()
    {
        var users = await _authService.GetAllUsersAsync();
        return users
            .Select(u => (u.RoleId, u.Role.Name))
            .Distinct()
            .ToList();
    }
}
