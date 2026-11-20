using Api.Endpoints;
using Core.Abstractions;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(); // Формат помилок RFC 9457

// Читаємо рядок підключення та будуємо абсолютний шлях від каталогу застосунку
var raw = builder.Configuration.GetConnectionString("AppDb") ?? "Data Source=data/app.db";
var csb = new SqliteConnectionStringBuilder(raw);
if (!Path.IsPathRooted(csb.DataSource))
{
    var full = Path.Combine(builder.Environment.ContentRootPath, csb.DataSource);
    Directory.CreateDirectory(Path.GetDirectoryName(full)!);
    csb.DataSource = full;
}

// Реєструємо контекст БД і сховище на EF Core з життєвим циклом Scoped
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(csb.ConnectionString));
builder.Services.AddScoped<ICatalogStore, EfCatalogStore>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Застосовуємо міграції EF Core у середовищі Development
using (var scope = app.Services.CreateScope())
{
    if (app.Environment.IsDevelopment())
    {
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Доступно за адресою /scalar/v1
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapProducts();

app.Run();