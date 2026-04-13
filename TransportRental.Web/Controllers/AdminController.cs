using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportRental.Core.Data;
using TransportRental.Core.Services;
using TransportRental.Web.Models;

namespace TransportRental.Web.Controllers;

[Authorize(Roles = "Администратор")]
public class AdminController : Controller
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _context;

    public AdminController(IAuthService authService, AppDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _authService.GetAllUsersAsync();
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadRoles();
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadRoles();
            return View(model);
        }

        try
        {
            await _authService.RegisterAsync(model.Username, model.Password, model.FullName, model.RoleId);
            TempData["Success"] = "Пользователь успешно создан";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadRoles();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _authService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        await LoadRoles();
        ViewBag.User = user;
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string fullName, int roleId, string? newPassword)
    {
        var user = await _authService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        user.FullName = fullName;
        user.RoleId = roleId;

        await _authService.UpdateUserAsync(user);

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            await _authService.ChangePasswordAsync(id, newPassword);
        }

        TempData["Success"] = "Пользователь обновлён";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var user = await _authService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        user.IsActive = !user.IsActive;
        await _authService.UpdateUserAsync(user);

        TempData["Success"] = user.IsActive
            ? "Пользователь активирован"
            : "Пользователь деактивирован";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadRoles()
    {
        var roles = await _context.Roles.OrderBy(r => r.Id).ToListAsync();
        ViewBag.Roles = roles.Select(r => (r.Id, r.Name)).ToList();
    }
}
