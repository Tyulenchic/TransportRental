using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportRental.Core.Models;
using TransportRental.Core.Services;

namespace TransportRental.Web.Controllers;

[Authorize]
public class VehiclesController : Controller
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId)
    {
        var vehicles = await _vehicleService.SearchAsync(search, categoryId, null);
        var categories = await _vehicleService.GetCategoriesAsync();

        ViewBag.Categories = categories;
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;

        return View(vehicles);
    }

    [Authorize(Roles = "Администратор,Менеджер")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _vehicleService.GetCategoriesAsync();
        return View(new Vehicle());
    }

    [Authorize(Roles = "Администратор,Менеджер")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Vehicle vehicle)
    {
        ModelState.Remove("Category");
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _vehicleService.GetCategoriesAsync();
            return View(vehicle);
        }

        await _vehicleService.CreateAsync(vehicle);
        TempData["Success"] = "Транспорт успешно добавлен";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Администратор,Менеджер")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);
        if (vehicle == null) return NotFound();

        ViewBag.Categories = await _vehicleService.GetCategoriesAsync();
        return View(vehicle);
    }

    [Authorize(Roles = "Администратор,Менеджер")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Vehicle vehicle)
    {
        if (id != vehicle.Id) return BadRequest();

        ModelState.Remove("Category");
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _vehicleService.GetCategoriesAsync();
            return View(vehicle);
        }

        await _vehicleService.UpdateAsync(vehicle);
        TempData["Success"] = "Транспорт успешно обновлён";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Администратор,Менеджер")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _vehicleService.DeleteAsync(id);
            TempData["Success"] = "Транспорт успешно удалён";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ошибка при удалении: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }
}
