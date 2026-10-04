using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            // Дсериіалізуємо список із JSON; якщо null — повертаємо порожній список
            var items = JsonSerializer.Deserialize<List<ProductDto>>(json, options) ?? new();
            
            // JSON-імпортер повертає завантажені елементи та порожній список помилок (або можна додати валідацію)
            return new ImportResult<ProductDto>(items, new List<string>());
        }
        catch (Exception ex)
        {
            // Якщо сталася помилка читання чи парсингу JSON, повертаємо її як помилку імпорту
            return new ImportResult<ProductDto>(new List<ProductDto>(), new List<string> { $"Помилка читання JSON: {ex.Message}" });
        }
    }
}