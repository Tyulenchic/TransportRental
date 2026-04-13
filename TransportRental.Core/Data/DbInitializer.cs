using Microsoft.EntityFrameworkCore;
using TransportRental.Core.Helpers;
using TransportRental.Core.Models;

namespace TransportRental.Core.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.Migrate();

        if (context.Roles.Any()) return;

        // Seed roles
        var adminRole = new Role { Name = "Администратор" };
        var managerRole = new Role { Name = "Менеджер" };
        var operatorRole = new Role { Name = "Оператор" };
        context.Roles.AddRange(adminRole, managerRole, operatorRole);
        context.SaveChanges();

        // Seed admin user (login: admin, password: admin123)
        var admin = new User
        {
            Username = "admin",
            PasswordHash = PasswordHasher.Hash("admin123"),
            FullName = "Администратор системы",
            RoleId = adminRole.Id,
            IsActive = true
        };
        context.Users.Add(admin);

        // Seed vehicle categories
        var categories = new[]
        {
            new VehicleCategory { Name = "Автомобиль", Description = "Легковые автомобили" },
            new VehicleCategory { Name = "Велосипед", Description = "Велосипеды различных типов" },
            new VehicleCategory { Name = "Самокат", Description = "Обычные самокаты" },
            new VehicleCategory { Name = "Электросамокат", Description = "Электрические самокаты" },
            new VehicleCategory { Name = "Мотоцикл", Description = "Мотоциклы и мопеды" }
        };
        context.VehicleCategories.AddRange(categories);
        context.SaveChanges();

        // Seed sample vehicles
        var vehicles = new[]
        {
            new Vehicle { Name = "Toyota Camry 2023", CategoryId = categories[0].Id, LicensePlate = "А001АА77", HourlyRate = 500, DailyRate = 3500, Year = 2023 },
            new Vehicle { Name = "Kia Rio 2022", CategoryId = categories[0].Id, LicensePlate = "В002ВВ77", HourlyRate = 350, DailyRate = 2500, Year = 2022 },
            new Vehicle { Name = "Горный велосипед Stels", CategoryId = categories[1].Id, HourlyRate = 100, DailyRate = 500, Year = 2023 },
            new Vehicle { Name = "Городской велосипед Forward", CategoryId = categories[1].Id, HourlyRate = 80, DailyRate = 400, Year = 2022 },
            new Vehicle { Name = "Самокат Razor A5", CategoryId = categories[2].Id, HourlyRate = 50, DailyRate = 300 },
            new Vehicle { Name = "Электросамокат Xiaomi Mi Pro 2", CategoryId = categories[3].Id, HourlyRate = 150, DailyRate = 900, Year = 2023 },
            new Vehicle { Name = "Электросамокат Ninebot Max G30", CategoryId = categories[3].Id, HourlyRate = 200, DailyRate = 1200, Year = 2023 },
            new Vehicle { Name = "Honda CB500F", CategoryId = categories[4].Id, LicensePlate = "С003СС77", HourlyRate = 600, DailyRate = 4000, Year = 2022 },
        };
        context.Vehicles.AddRange(vehicles);

        // Seed sample customers
        var customers = new[]
        {
            new Customer { FirstName = "Иван", LastName = "Петров", MiddleName = "Сергеевич", Phone = "+7-900-111-22-33", Email = "petrov@mail.ru", PassportNumber = "4510 123456", Address = "г. Москва, ул. Ленина, д. 10" },
            new Customer { FirstName = "Мария", LastName = "Сидорова", Phone = "+7-900-222-33-44", Email = "sidorova@mail.ru", PassportNumber = "4511 654321", Address = "г. Москва, ул. Пушкина, д. 5" },
            new Customer { FirstName = "Алексей", LastName = "Козлов", MiddleName = "Дмитриевич", Phone = "+7-900-333-44-55", PassportNumber = "4512 111222" },
        };
        context.Customers.AddRange(customers);
        context.SaveChanges();
    }
}
