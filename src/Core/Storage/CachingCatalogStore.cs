/*using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingCatalogStore(ICatalogStore innerStore) : ICatalogStore
{
    private readonly ICatalogStore _innerStore = innerStore ?? throw new ArgumentNullException(nameof(innerStore));
    
    // Локальний кеш у пам'яті для швидкого доступу
    private Dictionary<string, Product>? _cache;

    private Dictionary<string, Product> GetOrLoadCache()
    - {
        if (_cache == null)
        {
            // Якщо кеш пустий, завантажуємо дані з внутрішнього сховища (наприклад, файлу)
            _cache = _innerStore.List().ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
        }
        return _cache;
    }

    public IReadOnlyList<Product> List()
    {
        return GetOrLoadCache().Values.ToList();
    }

    public Product? GetById(string id)
    {
        return GetOrLoadCache().GetValueOrDefault(id);
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _innerStore.Add(item); // Делегуємо операцію внутрішньому сховищу
        GetOrLoadCache()[item.Id] = item; // Оновлюємо локальний кеш
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _innerStore.Update(item);
        GetOrLoadCache()[item.Id] = item;
    }

    public bool Remove(string id)
    {
        var removed = _innerStore.Remove(id);
        if (removed)
        {
            GetOrLoadCache().Remove(id);
        }
        return removed;
    }
}
*/