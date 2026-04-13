using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TransportRental.Core.Services;

namespace TransportRental.WPF.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        UsernameBox.Focus();
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        var username = UsernameBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Введите логин и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var scope = App.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var user = await authService.LoginAsync(username, password);
        if (user == null)
        {
            MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        App.CurrentUser = user;
        var mainWindow = new MainWindow();
        mainWindow.Show();
        Close();
    }
}
