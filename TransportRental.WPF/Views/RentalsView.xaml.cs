using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TransportRental.Core.Models;
using TransportRental.Core.Services;
using TransportRental.WPF.Views.Dialogs;

namespace TransportRental.WPF.Views;

public partial class RentalsView : UserControl
{
    private bool _isLoaded;

    public RentalsView()
    {
        InitializeComponent();
        Loaded += async (_, _) => { _isLoaded = true; await RefreshAsync(); };
    }

    private async Task RefreshAsync()
    {
        if (!_isLoaded) return;
        using var scope = App.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRentalService>();

        RentalStatus? status = null;
        if (StatusFilter.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            status = Enum.Parse<RentalStatus>(tag);

        var from = DateFrom.SelectedDate.HasValue ? DateFrom.SelectedDate.Value.ToUniversalTime() : (DateTime?)null;
        var to = DateTo.SelectedDate.HasValue ? DateTo.SelectedDate.Value.Date.AddDays(1).ToUniversalTime() : (DateTime?)null;

        // Also check for overdue
        await service.GetOverdueAsync();

        RentalsGrid.ItemsSource = await service.SearchAsync(status, from, to);
    }

    private async void StatusFilter_Changed(object sender, SelectionChangedEventArgs e) => await RefreshAsync();
    private async void DateFilter_Changed(object sender, SelectionChangedEventArgs e) => await RefreshAsync();

    private async void NewRental_Click(object sender, RoutedEventArgs e)
    {
        using var scope = App.Services.CreateScope();
        var vehicleService = scope.ServiceProvider.GetRequiredService<IVehicleService>();
        var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        var vehicles = await vehicleService.GetAvailableAsync();
        var customers = await customerService.GetAllAsync();

        if (!vehicles.Any())
        {
            MessageBox.Show("Нет доступного транспорта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new RentalEditDialog(vehicles, customers);
        if (dialog.ShowDialog() == true)
        {
            var rentalService = scope.ServiceProvider.GetRequiredService<IRentalService>();
            try
            {
                await rentalService.CreateRentalAsync(
                    dialog.SelectedVehicleId,
                    dialog.SelectedCustomerId,
                    App.CurrentUser!.Id,
                    dialog.ExpectedEndDate,
                    dialog.Notes);
                await RefreshAsync();
                MessageBox.Show("Прокат оформлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Return_Click(object sender, RoutedEventArgs e)
    {
        if (RentalsGrid.SelectedItem is not Rental selected || selected.Status == RentalStatus.Completed)
        {
            MessageBox.Show("Выберите активный прокат", "Информация", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var scope = App.Services.CreateScope();
        var rentalService = scope.ServiceProvider.GetRequiredService<IRentalService>();
        var price = rentalService.CalculatePrice(selected.Vehicle, selected.StartDate, DateTime.UtcNow);

        var result = MessageBox.Show(
            $"Вернуть '{selected.Vehicle.Name}'?\nКлиент: {selected.Customer.FullName}\nСумма к оплате: {price:N0} руб.",
            "Возврат транспорта", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            await rentalService.ReturnVehicleAsync(selected.Id, PaymentMethod.Cash);
            await RefreshAsync();
            MessageBox.Show($"Транспорт возвращён. Сумма: {price:N0} руб.", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async void Cancel_Click(object sender, RoutedEventArgs e)
    {
        if (RentalsGrid.SelectedItem is not Rental selected ||
            (selected.Status != RentalStatus.Active && selected.Status != RentalStatus.Overdue))
        {
            MessageBox.Show("Выберите активный прокат", "Информация", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MessageBox.Show("Отменить прокат?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

        using var scope = App.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRentalService>();
        await service.CancelRentalAsync(selected.Id);
        await RefreshAsync();
    }
}
