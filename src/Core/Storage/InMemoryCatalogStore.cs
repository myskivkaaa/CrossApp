using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryCatalogStore(IEnumerable<Product>? seed = null) : ICatalogStore
{
    // Зберігаю товари у словнику з регістронезалежними ключами для надійного пошуку за ID
    private readonly Dictionary<string, Product> _items = 
        (seed ?? []).ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Product> List() => _items.Values.ToList(); // Повертаю копію списку значень

    public Product? GetById(string id) => _items.GetValueOrDefault(id); // Повертаю товар або null, якщо не знайдено

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item); // Перевіряю, щоб переданий об'єкт не був null
        if (_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує."); // Запобігаю дублюванню ID
        
        _items.Add(item.Id, item);
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items[item.Id] = item; // Оновлюю або створюю запис у словнику за ключем ID
    }

    public bool Remove(string id) => _items.Remove(id); // Видаляю запис із словника та повертаю результат

    public IReadOnlyList<Product> FindBy(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _items.Values.Where(predicate).ToList();
    }
}