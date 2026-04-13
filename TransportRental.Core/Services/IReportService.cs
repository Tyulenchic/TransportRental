namespace TransportRental.Core.Services;

public class RevenueReportItem
{
    public DateTime Date { get; set; }
    public int RentalsCount { get; set; }
    public decimal Revenue { get; set; }
}

public class CategoryReportItem
{
    public string CategoryName { get; set; } = string.Empty;
    public int RentalsCount { get; set; }
    public decimal Revenue { get; set; }
}

public class PopularVehicleItem
{
    public string VehicleName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int RentalsCount { get; set; }
    public decimal TotalRevenue { get; set; }
}

public interface IReportService
{
    Task<List<RevenueReportItem>> GetRevenueReportAsync(DateTime from, DateTime to);
    Task<List<CategoryReportItem>> GetCategoryReportAsync(DateTime from, DateTime to);
    Task<List<PopularVehicleItem>> GetPopularVehiclesAsync(DateTime from, DateTime to, int top = 10);
    Task<byte[]> ExportRevenueToExcelAsync(DateTime from, DateTime to);
    Task<byte[]> ExportCategoryReportToExcelAsync(DateTime from, DateTime to);
    Task<byte[]> ExportRentalsToExcelAsync(DateTime from, DateTime to);
}
