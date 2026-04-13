using System.Windows;
using System.Windows.Controls;
using TransportRental.Core.Models;

namespace TransportRental.WPF.Views.Dialogs;

public partial class RentalEditDialog : Window
{
    public int SelectedVehicleId { get; private set; }
    public int SelectedCustomerId { get; private set; }
    public DateTime ExpectedEndDate { get; private set; }
    public string? Notes { get; private set; }

    public RentalEditDialog(List<Vehicle> vehicles, List<Customer> customers)
    {
        InitializeComponent();
        VehicleBox.ItemsSource = vehicles;
        CustomerBox.ItemsSource = customers;
        ExpectedEndBox.SelectedDate = DateTime.Now.AddDays(1);
    }

    private void Vehicle_Changed(object sender, SelectionChangedEventArgs e) => UpdatePrice();
    private void Date_Changed(object sender, SelectionChangedEventArgs e) => UpdatePrice();

    private void UpdatePrice()
    {
        if (VehicleBox.SelectedItem is not Vehicle vehicle || !ExpectedEndBox.SelectedDate.HasValue) return;
        var start = DateTime.UtcNow;
        var end = ExpectedEndBox.SelectedDate.Value.ToUniversalTime();
        var duration = end - start;
        decimal price;
        if (duration.TotalHours <= 24)
            price = Math.Max(1, (int)Math.Ceiling(duration.TotalHours)) * vehicle.HourlyRate;
        else
            price = (int)Math.Ceiling(duration.TotalDays) * vehicle.DailyRate;
        PricePreview.Text = $"{price:N0} руб.";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (VehicleBox.SelectedItem is not Vehicle vehicle)
        {
            MessageBox.Show("Выберите транспорт", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (CustomerBox.SelectedItem is not Customer customer)
        {
            MessageBox.Show("Выберите клиента", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!ExpectedEndBox.SelectedDate.HasValue || ExpectedEndBox.SelectedDate.Value <= DateTime.Now)
        {
            MessageBox.Show("Выберите корректную дату возврата", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SelectedVehicleId = vehicle.Id;
        SelectedCustomerId = customer.Id;
        ExpectedEndDate = ExpectedEndBox.SelectedDate.Value.ToUniversalTime();
        Notes = string.IsNullOrWhiteSpace(NotesBox.Text) ? null : NotesBox.Text.Trim();
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
