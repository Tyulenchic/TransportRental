using System.Windows;
using TransportRental.Core.Models;

namespace TransportRental.WPF.Views.Dialogs;

public partial class UserEditDialog : Window
{
    public string Username { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public int RoleId { get; private set; }

    private readonly bool _isEdit;

    public UserEditDialog(User? user, List<Role> roles)
    {
        InitializeComponent();
        RoleBox.ItemsSource = roles;
        _isEdit = user != null;

        if (user != null)
        {
            UsernameBox.Text = user.Username;
            FullNameBox.Text = user.FullName;
            RoleBox.SelectedItem = roles.FirstOrDefault(r => r.Id == user.RoleId);
        }

        Title = _isEdit ? "Редактирование пользователя" : "Добавление пользователя";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UsernameBox.Text) || string.IsNullOrWhiteSpace(FullNameBox.Text))
        {
            MessageBox.Show("Заполните логин и ФИО", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!_isEdit && string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (RoleBox.SelectedItem is not Role role)
        {
            MessageBox.Show("Выберите роль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Username = UsernameBox.Text.Trim();
        FullName = FullNameBox.Text.Trim();
        Password = PasswordBox.Password;
        RoleId = role.Id;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
