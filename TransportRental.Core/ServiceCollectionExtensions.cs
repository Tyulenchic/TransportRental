using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TransportRental.Core.Data;
using TransportRental.Core.Data.Repositories;
using TransportRental.Core.Services;

namespace TransportRental.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransportRentalCore(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IRentalService, RentalService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
