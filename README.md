# Прокат транспорта — Информационная система

Система для управления прокатом транспортных средств (автомобили, велосипеды, самокаты, мотоциклы).  
Включает **десктопное приложение (WPF)** и **веб-приложение (ASP.NET MVC)** с общей базой данных PostgreSQL.

## Стек технологий

- .NET 8, C#
- WPF (десктоп)
- ASP.NET MVC + Bootstrap 5 (веб)
- PostgreSQL + Entity Framework Core
- ClosedXML (экспорт в Excel)

## Требования

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) (любая актуальная версия)
- EF Core Tools:
  ```bash
  dotnet tool install --global dotnet-ef --version 8.0.*
  ```

## Быстрый старт

### 1. Клонирование

```bash
git clone https://github.com/kiragitstudy/TransportRental.git
cd TransportRental
```

### 2. Настройка базы данных

Убедитесь, что PostgreSQL запущен. Измените строку подключения под свои данные:

**Для WPF** — файл `TransportRental.WPF/App.xaml.cs`, строка:
```csharp
private const string ConnectionString = "Host=localhost;Port=5432;Database=transport_rental;Username=postgres;Password=ВАШ_ПАРОЛЬ";
```

**Для Web** — файл `TransportRental.Web/appsettings.json`:
```json
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=transport_rental;Username=postgres;Password=ВАШ_ПАРОЛЬ"
}
```

### 3. Применение миграций (создание БД)

```bash
dotnet ef database update --project TransportRental.Core --startup-project TransportRental.Web
```

База `transport_rental` создастся автоматически со всеми таблицами.  
При первом запуске приложения также заполнятся начальные данные (роли, админ, категории, тестовый транспорт и клиенты).

### 4. Запуск

**Веб-приложение:**
```bash
cd TransportRental.Web
dotnet run
```
Откроется на `https://localhost:5001` (или порт из консоли).

**Десктоп-приложение:**
```bash
cd TransportRental.WPF
dotnet run
```

### 5. Вход в систему

| Логин | Пароль   | Роль           |
|-------|----------|----------------|
| admin | admin123 | Администратор  |

Новых пользователей можно создать через раздел «Пользователи» (только админ).

## Структура проекта

```
TransportRental.Core/     — общая библиотека (модели, БД, сервисы)
TransportRental.WPF/      — десктопное приложение
TransportRental.Web/      — веб-приложение
```

## Роли пользователей

| Роль           | Возможности |
|----------------|-------------|
| Администратор  | Полный доступ + управление пользователями |
| Менеджер       | Транспорт, клиенты, прокаты, отчёты, экспорт |
| Оператор       | Просмотр транспорта, клиенты, оформление/возврат проката |

## Функционал

- Управление транспортом и категориями (CRUD)
- Управление клиентами (CRUD)
- Оформление и возврат проката с расчётом стоимости
- Контроль просроченных прокатов
- Учёт оплат (наличные / карта / перевод)
- Отчёты: доход за период, по категориям, популярный транспорт
- Экспорт отчётов в Excel
- Ролевой доступ
