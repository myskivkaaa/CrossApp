using System.Text.Json;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

// Імпортер повертає загальний результат із сутностями IEntityItem
// Вибираємо імпортер залежно від розширення файлу та приводимо результати до ProductDto
ImportResult<ProductDto> result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => ProductCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    var ext => new ImportResult<ProductDto>(
        new List<ProductDto>(), 
        new List<string> { $"Невідомий формат файлу: {ext}" })
};

Console.WriteLine($"Завантажено записів: {result.Items.Count}");

// Виводимо лише ті елементи, які є товарами (ProductDto), для красивого виводу в консоль
foreach (var item in result.Items.OfType<ProductDto>().Take(5))
{
    Console.WriteLine($"  {item.Id,-6} {item.Sku,-10} {item.Name,-26} {item.Quantity,5} {item.Unit}");
}

// Якщо є склади (WarehouseDto), також можемо показати їхню кількість або приклади
var warehouses = result.Items.OfType<WarehouseDto>().ToList();
if (warehouses.Count > 0)
{
    Console.WriteLine($"Завантажено складів: {warehouses.Count}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($"  ! {e}");
    }
}

// Статистика імпорту (Додаткове завдання 3)
int totalProcessed = result.Items.Count + result.Errors.Count;
double errorPercentage = totalProcessed > 0 ? (double)result.Errors.Count / totalProcessed * 100 : 0;
Console.WriteLine($"[Статистика імпорту] Усього: {totalProcessed} | Прийнято: {result.Items.Count} | Пропущено: {result.Errors.Count} | Помилки: {errorPercentage:F1}%");

return 0;