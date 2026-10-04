using System.Text.Json;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Отримуємо шлях до файлу з аргументів або використовуємо шлях за замовчуванням (.csv або .json)
string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

// Вибираємо імпортер залежно від розширення файлу за допомогою switch expression (Додаткове завдання 1)
ImportResult<ProductDto> result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => ProductCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    var ext => new ImportResult<ProductDto>(
        new List<ProductDto>(), 
        new List<string> { $"Невідомий формат файлу: {ext}" })
};

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (ProductDto p in result.Items.Take(5))
{
    Console.WriteLine($"  {p.Id,-6} {p.Sku,-10} {p.Name,-26} {p.Quantity,5} {p.Unit}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($"  ! {e}");
    }
}

// Виведення статистики імпорту одним рядком (Додаткове завдання 3)
int totalProcessed = result.Items.Count + result.Errors.Count;
double errorPercentage = totalProcessed > 0 ? (double)result.Errors.Count / totalProcessed * 100 : 0;
Console.WriteLine($"[Статистика імпорту] Усього: {totalProcessed} | Прийнято: {result.Items.Count} | Пропущено: {result.Errors.Count} | Помилки: {errorPercentage:F1}%");

return 0;