using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingCatalogStore(ICatalogStore innerStore) : ICatalogStore
{
    // Зберігаю посилання на внутрішнє сховище (наприклад, файлове), перевіряючи його на null
    private readonly ICatalogStore _innerStore = innerStore ?? throw new ArgumentNullException(nameof(innerStore));
    
    // Локальний кеш у пам'яті для швидкого доступу без зайвих звернень до диску
    private Dictionary<string, Product>? _cache;

    // Приватний метод для ініціалізації або отримання вже завантаженого кешу
    private Dictionary<string, Product> GetOrLoadCache()
    {
        if (_cache == null)
        {
            // Якщо кеш порожній, завантажуємо дані з внутрішнього сховища та перетворюємо у словник
            _cache = _innerStore.List().ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
        }
        return _cache;
    }

    // Отримання списку всіх товарів (працює через швидкий кеш)
    public IReadOnlyList<Product> List()
    {
        return GetOrLoadCache().Values.ToList();
    }

    // Пошук товару за унікальним ID через кеш
    public Product? GetById(string id)
    {
        return GetOrLoadCache().GetValueOrDefault(id);
    }

    // Додавання нового товару (делегуємо внутрішньому сховищу і синхронізуємо локальний кеш)
    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _innerStore.Add(item); // Зберігаємо у внутрішньому сховищі
        GetOrLoadCache()[item.Id] = item; // Додаємо до локального кешу
    }

    // Оновлення існуючого товару
    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _innerStore.Update(item); // Оновлюємо у внутрішньому сховищі
        GetOrLoadCache()[item.Id] = item; // Оновлюємо в локальному кеші
    }

    // Видалення товару за ID
    public bool Remove(string id)
    {
        var removed = _innerStore.Remove(id); // Видаляємо з внутрішнього сховища
        if (removed)
        {
            GetOrLoadCache().Remove(id); // Видаляємо з локального кешу, якщо операція успішна
        }
        return removed;
    }

    // Пошук за предикатом (фільтрація даних з використанням кешу)
    public IReadOnlyList<Product> FindBy(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return GetOrLoadCache().Values.Where(predicate).ToList();
    }
}