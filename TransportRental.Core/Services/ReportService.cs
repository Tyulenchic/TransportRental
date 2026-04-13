using Microsoft.EntityFrameworkCore;
using TransportRental.Core.Data;
using TransportRental.Core.Helpers;
using TransportRental.Core.Models;

namespace TransportRental.Core.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RevenueReportItem>> GetRevenueReportAsync(DateTime from, DateTime to)
    {
        return await _context.Rentals
            .Where(r => r.Status == RentalStatus.Completed && r.EndDate >= from && r.EndDate <= to)
            .GroupBy(r => r.EndDate!.Value.Date)
            .Select(g => new RevenueReportItem
            {
                Date = g.Key,
                RentalsCount = g.Count(),
                Revenue = g.Sum(r => r.TotalPrice ?? 0)
            })
            .OrderBy(r => r.Date)
            .ToListAsync();
    }

    public async Task<List<CategoryReportItem>> GetCategoryReportAsync(DateTime from, DateTime to)
    {
        return await _context.Rentals
            .Include(r => r.Vehicle).ThenInclude(v => v.Category)
            .Where(r => r.Status == RentalStatus.Completed && r.EndDate >= from && r.EndDate <= to)
            .GroupBy(r => r.Vehicle.Category.Name)
            .Select(g => new CategoryReportItem
            {
                CategoryName = g.Key,
                RentalsCount = g.Count(),
                Revenue = g.Sum(r => r.TotalPrice ?? 0)
            })
            .OrderByDescending(r => r.Revenue)
            .ToListAsync();
    }

    public async Task<List<PopularVehicleItem>> GetPopularVehiclesAsync(DateTime from, DateTime to, int top = 10)
    {
        return await _context.Rentals
            .Include(r => r.Vehicle).ThenInclude(v => v.Category)
            .Where(r => r.StartDate >= from && r.StartDate <= to)
            .GroupBy(r => new { r.Vehicle.Name, CategoryName = r.Vehicle.Category.Name })
            .Select(g => new PopularVehicleItem
            {
                VehicleName = g.Key.Name,
                CategoryName = g.Key.CategoryName,
                RentalsCount = g.Count(),
                TotalRevenue = g.Sum(r => r.TotalPrice ?? 0)
            })
            .OrderByDescending(r => r.RentalsCount)
            .Take(top)
            .ToListAsync();
    }

    public async Task<byte[]> ExportRevenueToExcelAsync(DateTime from, DateTime to)
    {
        var data = await GetRevenueReportAsync(from, to);
        return ExcelExporter.ExportToExcel(data, "Доход", new Dictionary<string, Func<RevenueReportItem, object?>>
        {
            ["Дата"] = r => r.Date,
            ["Кол-во прокатов"] = r => r.RentalsCount,
            ["Доход (руб.)"] = r => r.Revenue
        });
    }

    public async Task<byte[]> ExportCategoryReportToExcelAsync(DateTime from, DateTime to)
    {
        var data = await GetCategoryReportAsync(from, to);
        return ExcelExporter.ExportToExcel(data, "По категориям", new Dictionary<string, Func<CategoryReportItem, object?>>
        {
            ["Категория"] = r => r.CategoryName,
            ["Кол-во прокатов"] = r => r.RentalsCount,
            ["Доход (руб.)"] = r => r.Revenue
        });
    }

    public async Task<byte[]> ExportRentalsToExcelAsync(DateTime from, DateTime to)
    {
        var rentals = await _context.Rentals
            .Include(r => r.Vehicle).ThenInclude(v => v.Category)
            .Include(r => r.Customer)
            .Include(r => r.User)
            .Where(r => r.StartDate >= from && r.StartDate <= to)
            .OrderByDescending(r => r.StartDate)
            .ToListAsync();

        return ExcelExporter.ExportToExcel(rentals, "Прокаты", new Dictionary<string, Func<Rental, object?>>
        {
            ["ID"] = r => r.Id,
            ["Транспорт"] = r => r.Vehicle.Name,
            ["Категория"] = r => r.Vehicle.Category.Name,
            ["Клиент"] = r => r.Customer.FullName,
            ["Сотрудник"] = r => r.User.FullName,
            ["Дата начала"] = r => r.StartDate,
            ["Дата возврата"] = r => r.EndDate,
            ["Статус"] = r => r.Status.ToString(),
            ["Сумма (руб.)"] = r => r.TotalPrice
        });
    }
}
