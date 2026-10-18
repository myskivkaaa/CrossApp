/*using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileCatalogStore(string path) : ICatalogStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true }; // Форматування JSON з відступами
    private readonly Dictionary<string, Product> _cache = new(StringComparer.OrdinalIgnoreCase); // Кеш у пам'яті для швидкого доступу
    private readonly string _path = Path.GetFullPath(path); // Абсолютний шлях до файлу даних
    private bool _loaded; // Прапорець, щоб завантажувати файл лише один раз при першому зверненні

    private void EnsureLoaded()
    {
        if (_loaded) return;

        if (File.Exists(_path))
        {
            // Зчитую JSON-файл і десеріалізую його у список DTO-об'єктів
            var json = File.ReadAllText(_path);
            var dtos = JsonSerializer.Deserialize<List<ProductDto>>(json) ?? [];
            
            // Відновлюю повноцінні доменні сутності через FromDto з перевіркою інваріантів
            foreach (var dto in dtos)
            {
                var p = Product.FromDto(dto);
                _cache[p.Id] = p;
            }
        }
        _loaded = true;
    }

    private void Flush()
    {
        // Гарантую, що каталог для файлу існує, перш ніж записувати дані
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        
        // Перетворюю сутності в DTO для коректної серіалізації без загрози приватним сетерам
        var dtos = _cache.Values.Select(p => p.ToDto()).ToList();
        var json = JsonSerializer.Serialize(dtos, Options);
        
        File.WriteAllText(_path, json); // Записую актуальний стан на диск
    }

    public IReadOnlyList<Product> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Product? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        
        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");
            
        _cache.Add(item.Id, item);
        Flush(); // Зберігаю зміни на диск одразу після додавання
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        
        _cache[item.Id] = item;
        Flush(); // Зберігаю зміни на диск після оновлення
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        
        if (!_cache.Remove(id)) return false;
        
        Flush(); // Зберігаю зміни на диск після видалення
        return true;
    }
}
*/