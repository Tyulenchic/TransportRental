using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportRental.Core.Models;
using TransportRental.Core.Services;

namespace TransportRental.Web.Controllers;

[Authorize]
public class RentalsController : Controller
{
    private readonly IRentalService _rentalService;
    private readonly IVehicleService _vehicleService;
    private readonly ICustomerService _customerService;

    public RentalsController(
        IRentalService rentalService,
        IVehicleService vehicleService,
        ICustomerService customerService)
    {
        _rentalService = rentalService;
        _vehicleService = vehicleService;
        _customerService = customerService;
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirst("UserId")?.Value ?? "0");

    public async Task<IActionResult> Index(RentalStatus? status, DateTime? dateFrom, DateTime? dateTo)
    {
        var rentals = await _rentalService.SearchAsync(status, dateFrom, dateTo);

        ViewBag.Status = status;
        ViewBag.DateFrom = dateFrom;
        ViewBag.DateTo = dateTo;

        return View(rentals);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Vehicles = await _vehicleService.GetAvailableAsync();
        ViewBag.Customers = await _customerService.GetAllAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int vehicleId, int customerId, DateTime expectedEndDate, string? notes)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _rentalService.CreateRentalAsync(vehicleId, customerId, userId, expectedEndDate, notes);
            TempData["Success"] = "Прокат успешно оформлен";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ошибка: {ex.Message}";
            ViewBag.Vehicles = await _vehicleService.GetAvailableAsync();
            ViewBag.Customers = await _customerService.GetAllAsync();
            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Return(int id)
    {
        var rental = await _rentalService.GetByIdAsync(id);
        if (rental == null) return NotFound();
        if (rental.Status != RentalStatus.Active && rental.Status != RentalStatus.Overdue)
        {
            TempData["Error"] = "Данный прокат не может быть возвращён";
            return RedirectToAction(nameof(Index));
        }

        var price = _rentalService.CalculatePrice(rental.Vehicle, rental.StartDate, DateTime.Now);
        ViewBag.CalculatedPrice = price;

        return View(rental);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(int id, PaymentMethod paymentMethod)
    {
        try
        {
            await _rentalService.ReturnVehicleAsync(id, paymentMethod);
            TempData["Success"] = "Транспорт успешно возвращён";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ошибка: {ex.Message}";
            return RedirectToAction(nameof(Return), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _rentalService.CancelRentalAsync(id);
            TempData["Success"] = "Прокат отменён";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ошибка: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }
}
