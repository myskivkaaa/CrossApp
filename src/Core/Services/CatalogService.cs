using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class CatalogService(ICatalogStore store)
{
    // Зберігаю посилання на інтерфейс сховища через ін'єкцію в конструкторі
    private readonly ICatalogStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public Product Add(string sku, string name, string unit, int quantity)
    {
        // Генерую унікальний короткий ID та створюю сутність (усі бізнес-правила перевіряє фабрика Product.Create)
        var product = Product.Create(Guid.NewGuid().ToString("N")[..8], sku, name, unit, quantity);
        _store.Add(product);
        return product;
    }

    public void Receive(string id, int quantity)
    {
        // Знаходжу товар або кидаю виняток, якщо запис відсутній
        var product = _store.GetById(id) 
                      ?? throw new InvalidOperationException($"Немає запису з id={id}.");
                      
        product.RegisterArrival(quantity); // Змінюю стан за допомогою бізнес-методу сутності
        _store.Update(product);            // Зберігаю оновлений товар у сховищі
    }

    public IReadOnlyList<Product> All() => _store.List(); // Отримую загальний список через сховище

    public Product? Find(string id) => _store.GetById(id);  // Шукаю товар за ID

    // Метод пошуку за предикатом через сервіс
    public IReadOnlyList<Product> FindBy(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.FindBy(predicate);
    }
}