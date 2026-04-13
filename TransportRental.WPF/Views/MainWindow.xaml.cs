using System.Windows;

namespace TransportRental.WPF.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SetupRoleAccess();
    }

    private void SetupRoleAccess()
    {
        var user = App.CurrentUser;
        if (user == null) return;

        UserText.Text = $"{user.FullName} ({user.Role.Name})";
        var role = user.Role.Name;

        // Operator: no reports, no users
        if (role == "Оператор")
        {
            ReportsTab.Visibility = Visibility.Collapsed;
            UsersTab.Visibility = Visibility.Collapsed;
        }
        // Manager: no users
        else if (role == "Менеджер")
        {
            UsersTab.Visibility = Visibility.Collapsed;
        }
        // Admin: everything visible
    }
}
