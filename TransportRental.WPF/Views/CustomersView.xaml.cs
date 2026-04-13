using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TransportRental.Core.Models;
using TransportRental.Core.Services;
using TransportRental.WPF.Views.Dialogs;

namespace TransportRental.WPF.Views;

public partial class CustomersView : UserControl
{
    public CustomersView()
    {
        InitializeComponent();
        Loaded += async (_, _) => await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        using var scope = App.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICustomerService>();
        var query = SearchBox.Text.Trim();
        CustomersGrid.ItemsSource = string.IsNullOrEmpty(query)
            ? await service.GetAllAsync()
            : await service.SearchAsync(query);
    }

    private async void Search_TextChanged(object sender, TextChangedEventArgs e) => await RefreshAsync();

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CustomerEditDialog(null);
        if (dialog.ShowDialog() == true)
        {
            using var scope = App.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ICustomerService>();
            await service.CreateAsync(dialog.Customer);
            await RefreshAsync();
        }
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (CustomersGrid.SelectedItem is not Customer selected) return;
        var dialog = new CustomerEditDialog(selected);
        if (dialog.ShowDialog() == true)
        {
            using var scope = App.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ICustomerService>();
            await service.UpdateAsync(dialog.Customer);
            await RefreshAsync();
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (CustomersGrid.SelectedItem is not Customer selected) return;
        if (MessageBox.Show($"Удалить клиента '{selected.FullName}'?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            using var scope = App.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ICustomerService>();
            await service.DeleteAsync(selected.Id);
            await RefreshAsync();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
