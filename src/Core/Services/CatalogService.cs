using Core.Abstractions;
using Core.Domain;
using Core.Events; //  підключаємо простір імен для подій

namespace Core.Services;

public sealed class CatalogService(ICatalogStore store) // DI передаємо ззовні сховище а не сервіс створює його
{
    // Зберігаю посилання на інтерфейс сховища через ін'єкцію в конструкторі
    private readonly ICatalogStore _store = store ?? throw new ArgumentNullException(nameof(store));

    // НОВЕ: Подія для сповіщення зовнішнього коду про зміни стану
    public event EventHandler<CatalogChangedEventArgs>? Changed;

    // НОВЕ: Приватний метод підняття події
    private void OnChanged(Product p, CatalogChangeKind kind, int delta) =>
        Changed?.Invoke(this, new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity));

    public Product Add(string sku, string name, string unit, int quantity)
    {
        // Генерую унікальний короткий ID та створюю сутність (усі бізнес-правила перевіряє фабрика Product.Create)
        var product = Product.Create(Guid.NewGuid().ToString("N")[..8], sku, name, unit, quantity);
        _store.Add(product);
        
        // НОВЕ: Піднімаємо подію після успішного додавання
        OnChanged(product, CatalogChangeKind.Added, quantity); 
        
        return product;
    }

    public void Receive(string id, int quantity)
    {
        // Знаходжу товар або кидаю виняток, якщо запис відсутній
        var product = _store.GetById(id) 
                    ?? throw new InvalidOperationException($"Немає запису з id={id}.");
                    
        product.RegisterArrival(quantity); // Змінюю стан за допомогою бізнес-методу сутності
        _store.Update(product);            // Зберігаю оновлений товар у сховищі
        
        // НОВЕ: Піднімаємо подію після успішного надходження (якщо верхній рядок кине виняток, подія не виникне)
        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    public IReadOnlyList<Product> All() => _store.List(); // Отримую загальний список через сховище

    public Product? Find(string id) => _store.GetById(id);  // Шукаю товар за ID

    // Метод пошуку за предикатом через сервіс (або FindAll за вимогами завдання)
    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate) =>
        _store.List().Where(predicate).ToList();

    // Залишаємо і ваш попередній метод, якщо він використовується в інших місцях проєкту
    public IReadOnlyList<Product> FindBy(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.FindBy(predicate);
    }

    // ДОДАТКОВЕ ЗАВДАННЯ 2: Метод імпорту з Action<string> для звітів про пропущені рядки
public int ImportProducts(IEnumerable<(string sku, string name, string unit, int quantity)> items, Action<string> reportSkipped)
{
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(reportSkipped);

    int importedCount = 0;
    foreach (var item in items)
    {
        try
        {
            // Пробуємо створити товар (інваріанти сутності можуть кинути виняток)
            var product = Product.Create(Guid.NewGuid().ToString("N")[..8], item.sku, item.name, item.unit, item.quantity);
            _store.Add(product);
            OnChanged(product, CatalogChangeKind.Added, item.quantity);
            importedCount++;
        }
        catch (Exception ex)
        {
            // Передаємо інформацію про пропущений рядок через делегат Action<string>
            reportSkipped($"Пропущено рядок [SKU: {item.sku}, Назва: {item.name}]: {ex.Message}");
        }
    }
    return importedCount;
}
}