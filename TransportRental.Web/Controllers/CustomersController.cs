using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportRental.Core.Models;
using TransportRental.Core.Services;

namespace TransportRental.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var customers = string.IsNullOrWhiteSpace(search)
            ? await _customerService.GetAllAsync()
            : await _customerService.SearchAsync(search);

        ViewBag.Search = search;
        return View(customers);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Customer());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        ModelState.Remove("Rentals");
        if (!ModelState.IsValid)
            return View(customer);

        await _customerService.CreateAsync(customer);
        TempData["Success"] = "Клиент успешно добавлен";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null) return NotFound();
        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Customer customer)
    {
        if (id != customer.Id) return BadRequest();

        ModelState.Remove("Rentals");
        if (!ModelState.IsValid)
            return View(customer);

        await _customerService.UpdateAsync(customer);
        TempData["Success"] = "Данные клиента обновлены";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Администратор,Менеджер")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _customerService.DeleteAsync(id);
            TempData["Success"] = "Клиент успешно удалён";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ошибка при удалении: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }
}
