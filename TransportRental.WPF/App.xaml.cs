using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TransportRental.Core;
using TransportRental.Core.Data;
using TransportRental.WPF.Views;

namespace TransportRental.WPF;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static Core.Models.User? CurrentUser { get; set; }

    private const string ConnectionString = "Host=localhost;Port=5432;Database=transport_rental;Username=postgres;Password=test";

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransportRentalCore(ConnectionString);
        Services = serviceCollection.BuildServiceProvider();

        // Initialize database
        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            DbInitializer.Initialize(context);
        }

        var loginWindow = new LoginWindow();
        loginWindow.Show();
    }
}
