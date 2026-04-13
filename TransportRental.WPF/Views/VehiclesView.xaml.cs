using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TransportRental.Core.Models;
using TransportRental.Core.Services;
using TransportRental.WPF.Views.Dialogs;

namespace TransportRental.WPF.Views;

public partial class VehiclesView : UserControl
{
    private List<VehicleCategory> _categories = new();
    private bool _isLoaded;

    public VehiclesView()
    {
        InitializeComponent();
        Loaded += async (_, _) => { _isLoaded = true; await LoadDataAsync(); };
        SetupRoleAccess();
    }

    private void SetupRoleAccess()
    {
        var role = App.CurrentUser?.Role.Name;
        if (role == "Оператор")
        {
            AddButton.Visibility = Visibility.Collapsed;
            EditButton.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
        }
    }

    private async Task LoadDataAsync()
    {
        using var scope = App.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IVehicleService>();
        _categories = await service.GetCategoriesAsync();

        var allItem = new VehicleCategory { Id = 0, Name = "Все категории" };
        CategoryFilter.ItemsSource = new[] { allItem }.Concat(_categories).ToList();
        CategoryFilter.SelectedIndex = 0;

        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (!_isLoaded) return;
        using var scope = App.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IVehicleService>();
        var query = SearchBox.Text.Trim();
        var categoryId = (CategoryFilter.SelectedItem as VehicleCategory)?.Id;
        if (categoryId == 0) categoryId = null;
        var availableOnly = AvailableOnly.IsChecked == true;

        VehiclesGrid.ItemsSource = await service.SearchAsync(
            string.IsNullOrEmpty(query) ? null : query, categoryId, availableOnly ? true : null);
    }

    private async void Search_TextChanged(object sender, TextChangedEventArgs e) => await RefreshAsync();
    private async void CategoryFilter_Changed(object sender, SelectionChangedEventArgs e) => await RefreshAsync();
    private async void Filter_Changed(object sender, RoutedEventArgs e) => await RefreshAsync();
    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new VehicleEditDialog(null, _categories);
        if (dialog.ShowDialog() == true)
        {
            using var scope = App.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IVehicleService>();
            await service.CreateAsync(dialog.Vehicle);
            await RefreshAsync();
        }
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (VehiclesGrid.SelectedItem is not Vehicle selected) return;
        var dialog = new VehicleEditDialog(selected, _categories);
        if (dialog.ShowDialog() == true)
        {
            using var scope = App.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IVehicleService>();
            await service.UpdateAsync(dialog.Vehicle);
            await RefreshAsync();
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (VehiclesGrid.SelectedItem is not Vehicle selected) return;
        if (MessageBox.Show($"Удалить '{selected.Name}'?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            using var scope = App.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IVehicleService>();
            await service.DeleteAsync(selected.Id);
            await RefreshAsync();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
