using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportRental.Core.Services;

namespace TransportRental.Web.Controllers;

[Authorize(Roles = "Администратор,Менеджер")]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IActionResult> Index(string? reportType, DateTime? dateFrom, DateTime? dateTo)
    {
        dateFrom ??= DateTime.Today.AddMonths(-1);
        dateTo ??= DateTime.Today;

        ViewBag.ReportType = reportType;
        ViewBag.DateFrom = dateFrom.Value;
        ViewBag.DateTo = dateTo.Value;

        if (!string.IsNullOrEmpty(reportType))
        {
            switch (reportType)
            {
                case "revenue":
                    ViewBag.RevenueData = await _reportService.GetRevenueReportAsync(dateFrom.Value, dateTo.Value);
                    break;
                case "category":
                    ViewBag.CategoryData = await _reportService.GetCategoryReportAsync(dateFrom.Value, dateTo.Value);
                    break;
                case "popular":
                    ViewBag.PopularData = await _reportService.GetPopularVehiclesAsync(dateFrom.Value, dateTo.Value);
                    break;
            }
        }

        return View();
    }

    public async Task<IActionResult> ExportRevenue(DateTime dateFrom, DateTime dateTo)
    {
        var bytes = await _reportService.ExportRevenueToExcelAsync(dateFrom, dateTo);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "revenue_report.xlsx");
    }

    public async Task<IActionResult> ExportCategory(DateTime dateFrom, DateTime dateTo)
    {
        var bytes = await _reportService.ExportCategoryReportToExcelAsync(dateFrom, dateTo);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "category_report.xlsx");
    }

    public async Task<IActionResult> ExportRentals(DateTime dateFrom, DateTime dateTo)
    {
        var bytes = await _reportService.ExportRentalsToExcelAsync(dateFrom, dateTo);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "rentals_report.xlsx");
    }
}
