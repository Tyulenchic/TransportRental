using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportRental.Core.Services;
using TransportRental.Web.Models;

namespace TransportRental.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRentalService _rentalService;
    private readonly IVehicleService _vehicleService;
    private readonly ICustomerService _customerService;

    public HomeController(
        ILogger<HomeController> logger,
        IRentalService rentalService,
        IVehicleService vehicleService,
        ICustomerService customerService)
    {
        _logger = logger;
        _rentalService = rentalService;
        _vehicleService = vehicleService;
        _customerService = customerService;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var activeRentals = await _rentalService.GetActiveAsync();
        var overdueRentals = await _rentalService.GetOverdueAsync();
        var vehicles = await _vehicleService.GetAllAsync();
        var customers = await _customerService.GetAllAsync();

        ViewBag.ActiveRentals = activeRentals.Count;
        ViewBag.OverdueRentals = overdueRentals.Count;
        ViewBag.TotalVehicles = vehicles.Count;
        ViewBag.TotalCustomers = customers.Count;

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
