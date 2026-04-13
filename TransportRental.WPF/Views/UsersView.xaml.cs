using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using TransportRental.Core.Data;
using TransportRental.Core.Models;
using TransportRental.Core.Services;

namespace TransportRental.WPF.Views;

public partial class UsersView : UserControl
{
    private List<Role> _roles = new();

    public UsersView()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        using var scope = App.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _roles = context.Roles.OrderBy(r => r.Name).ToList();
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        using var scope = App.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAuthService>();
        UsersGrid.ItemsSource = await service.GetAllUsersAsync();
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Views.Dialogs.UserEditDialog(null, _roles);
        if (dialog.ShowDialog() == true)
        {
            try
            {
                using var scope = App.Services.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IAuthService>();
                await service.RegisterAsync(dialog.Username, dialog.Password, dialog.FullName, dialog.RoleId);
                await RefreshAsync();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (UsersGrid.SelectedItem is not User selected) return;
        var dialog = new Views.Dialogs.UserEditDialog(selected, _roles);
        if (dialog.ShowDialog() == true)
        {
            using var scope = App.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var user = await service.GetUserByIdAsync(selected.Id);
            if (user == null) return;
            user.FullName = dialog.FullName;
            user.RoleId = dialog.RoleId;
            user.Username = dialog.Username;
            await service.UpdateUserAsync(user);
            if (!string.IsNullOrEmpty(dialog.Password))
                await service.ChangePasswordAsync(user.Id, dialog.Password);
            await RefreshAsync();
        }
    }

    private async void Deactivate_Click(object sender, RoutedEventArgs e)
    {
        if (UsersGrid.SelectedItem is not User selected) return;
        if (selected.Id == App.CurrentUser?.Id)
        {
            MessageBox.Show("Нельзя деактивировать себя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var scope = App.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var user = await service.GetUserByIdAsync(selected.Id);
        if (user == null) return;
        user.IsActive = !user.IsActive;
        await service.UpdateUserAsync(user);
        await RefreshAsync();
    }
}
