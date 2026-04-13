using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using TransportRental.Core.Services;

namespace TransportRental.WPF.Views;

public partial class ReportsView : UserControl
{
    public ReportsView()
    {
        InitializeComponent();
        DateFrom.SelectedDate = DateTime.Now.AddMonths(-1);
        DateTo.SelectedDate = DateTime.Now;
    }

    private string GetSelectedTag()
    {
        return (ReportType.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Revenue";
    }

    private (DateTime from, DateTime to) GetDateRange()
    {
        var from = DateFrom.SelectedDate?.ToUniversalTime() ?? DateTime.UtcNow.AddMonths(-1);
        var to = DateTo.SelectedDate?.Date.AddDays(1).ToUniversalTime() ?? DateTime.UtcNow;
        return (from, to);
    }

    private void ReportType_Changed(object sender, SelectionChangedEventArgs e) { }

    private async void Generate_Click(object sender, RoutedEventArgs e)
    {
        var (from, to) = GetDateRange();
        using var scope = App.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IReportService>();

        ReportGrid.AutoGenerateColumns = true;
        switch (GetSelectedTag())
        {
            case "Revenue":
                ReportGrid.ItemsSource = await service.GetRevenueReportAsync(from, to);
                break;
            case "Category":
                ReportGrid.ItemsSource = await service.GetCategoryReportAsync(from, to);
                break;
            case "Popular":
                ReportGrid.ItemsSource = await service.GetPopularVehiclesAsync(from, to);
                break;
        }
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        var (from, to) = GetDateRange();
        using var scope = App.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IReportService>();

        byte[] data;
        string defaultName;
        switch (GetSelectedTag())
        {
            case "Revenue":
                data = await service.ExportRevenueToExcelAsync(from, to);
                defaultName = "Отчёт_доход.xlsx";
                break;
            case "Category":
                data = await service.ExportCategoryReportToExcelAsync(from, to);
                defaultName = "Отчёт_категории.xlsx";
                break;
            default:
                data = await service.ExportRentalsToExcelAsync(from, to);
                defaultName = "Отчёт_прокаты.xlsx";
                break;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx",
            FileName = defaultName
        };
        if (dialog.ShowDialog() == true)
        {
            await File.WriteAllBytesAsync(dialog.FileName, data);
            MessageBox.Show("Файл сохранён!", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
