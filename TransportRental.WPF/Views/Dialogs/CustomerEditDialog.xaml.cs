using System.Windows;
using TransportRental.Core.Models;

namespace TransportRental.WPF.Views.Dialogs;

public partial class CustomerEditDialog : Window
{
    public Customer Customer { get; private set; }

    public CustomerEditDialog(Customer? customer)
    {
        InitializeComponent();
        Customer = customer ?? new Customer();

        if (customer != null)
        {
            LastNameBox.Text = customer.LastName;
            FirstNameBox.Text = customer.FirstName;
            MiddleNameBox.Text = customer.MiddleName;
            PhoneBox.Text = customer.Phone;
            EmailBox.Text = customer.Email;
            PassportBox.Text = customer.PassportNumber;
            AddressBox.Text = customer.Address;
        }

        Title = customer != null ? "Редактирование клиента" : "Добавление клиента";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(LastNameBox.Text) || string.IsNullOrWhiteSpace(FirstNameBox.Text))
        {
            MessageBox.Show("Введите фамилию и имя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(PhoneBox.Text))
        {
            MessageBox.Show("Введите телефон", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(PassportBox.Text))
        {
            MessageBox.Show("Введите номер паспорта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Customer.LastName = LastNameBox.Text.Trim();
        Customer.FirstName = FirstNameBox.Text.Trim();
        Customer.MiddleName = string.IsNullOrWhiteSpace(MiddleNameBox.Text) ? null : MiddleNameBox.Text.Trim();
        Customer.Phone = PhoneBox.Text.Trim();
        Customer.Email = string.IsNullOrWhiteSpace(EmailBox.Text) ? null : EmailBox.Text.Trim();
        Customer.PassportNumber = PassportBox.Text.Trim();
        Customer.Address = string.IsNullOrWhiteSpace(AddressBox.Text) ? null : AddressBox.Text.Trim();

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
