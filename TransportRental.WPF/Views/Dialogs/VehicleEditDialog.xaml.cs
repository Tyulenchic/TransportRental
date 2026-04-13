using System.Windows;
using TransportRental.Core.Models;

namespace TransportRental.WPF.Views.Dialogs;

public partial class VehicleEditDialog : Window
{
    public Vehicle Vehicle { get; private set; }
    private readonly bool _isEdit;

    public VehicleEditDialog(Vehicle? vehicle, List<VehicleCategory> categories)
    {
        InitializeComponent();
        CategoryBox.ItemsSource = categories;
        _isEdit = vehicle != null;
        Vehicle = vehicle ?? new Vehicle();

        if (_isEdit)
        {
            NameBox.Text = Vehicle.Name;
            CategoryBox.SelectedItem = categories.FirstOrDefault(c => c.Id == Vehicle.CategoryId);
            LicensePlateBox.Text = Vehicle.LicensePlate;
            HourlyRateBox.Text = Vehicle.HourlyRate.ToString("F0");
            DailyRateBox.Text = Vehicle.DailyRate.ToString("F0");
            YearBox.Text = Vehicle.Year?.ToString();
            DescriptionBox.Text = Vehicle.Description;
            IsAvailableBox.IsChecked = Vehicle.IsAvailable;
        }

        Title = _isEdit ? "Редактирование транспорта" : "Добавление транспорта";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("Введите название", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (CategoryBox.SelectedItem is not VehicleCategory category)
        {
            MessageBox.Show("Выберите категорию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!decimal.TryParse(HourlyRateBox.Text, out var hourly) || hourly <= 0)
        {
            MessageBox.Show("Введите корректный тариф/час", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!decimal.TryParse(DailyRateBox.Text, out var daily) || daily <= 0)
        {
            MessageBox.Show("Введите корректный тариф/день", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Vehicle.Name = NameBox.Text.Trim();
        Vehicle.CategoryId = category.Id;
        Vehicle.LicensePlate = string.IsNullOrWhiteSpace(LicensePlateBox.Text) ? null : LicensePlateBox.Text.Trim();
        Vehicle.HourlyRate = hourly;
        Vehicle.DailyRate = daily;
        Vehicle.Year = int.TryParse(YearBox.Text, out var year) ? year : null;
        Vehicle.Description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim();
        Vehicle.IsAvailable = IsAvailableBox.IsChecked == true;

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
