using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

// Встановлюємо кодування консолі для коректного відображення українських символів
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Крок 5: Визначаємо режим роботи за допомогою аргументу командного рядка --file та --cache
bool useFile = args.Contains("--file");
bool useCache = args.Contains("--cache");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

// Composition Root: єдине місце в додатку, де обирається конкретна реалізація сховища
ICatalogStore store = useFile
    ? (useCache ? new CachingCatalogStore(new FileCatalogStore(dataPath)) : new FileCatalogStore(dataPath))
    : new InMemoryCatalogStore(SampleData.Products());

// Впроваджуємо сховище в бізнес-сервіс через конструктор (Dependency Injection)
var service = new CatalogService(store);

Console.WriteLine("==================================================");
Console.WriteLine($"ПРОЄКТ КАТАЛОГУ (Сховище: {store.GetType().Name})");
Console.WriteLine("==================================================\n");

// Крок 6: Основний сценарій роботи через сервіс (успіх)
Console.WriteLine("ОСНОВНИЙ ХІД РОБОТИ: Операції через сервіс");
try
{
    var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
    Console.WriteLine($"Створено товар: {created}");

    service.Receive(created.Id, 25);
    Console.WriteLine($"Після приходу: {service.Find(created.Id)}");
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}
Console.WriteLine();

// Крок 6: Сценарій відмови (порушення інваріантів через try/catch)
Console.WriteLine("ОСНОВНИЙ ХІД РОБОТИ: Сценарій відмови");
TryDo("отримання приходу для неіснуючого ID", () => service.Receive("non-existent-id", 10));
TryDo("додавання товару з порожнім SKU", () => service.Add("", "Пісок", "т", 10));
TryDo("додавання товару з від'ємним залишком", () => service.Add("SKU-003", "Цегла", "шт", -5));
Console.WriteLine();

// Виведення поточного стану сховища через сервіс
Console.WriteLine("==================================================");
Console.WriteLine("ПОТОЧНИЙ СПИСОК ТОВАРІВ У СХОВИЩІ:");
Console.WriteLine("==================================================");
foreach (var p in service.All())
{
    Console.WriteLine($"{p.Id}  [{p.Sku,-8}]  {p.Name,-20}  -  {p.Quantity,3} {p.Unit}");
}
Console.WriteLine("==================================================\n");

// ДОДАТКОВІ ЗАВДАННЯ
Console.WriteLine("==================================================");
Console.WriteLine("ДОДАТКОВІ ЗАВДАННЯ:");
Console.WriteLine("==================================================");

// Додаткове завдання 1: Демонстрація кешуючого декоратора CachingCatalogStore
Console.WriteLine("Додаткове 1: Декоратор CachingCatalogStore");
Console.WriteLine($"Використовується обгортка кешування поверх сховища: {store.GetType().Name}");
Console.WriteLine();

// Додаткове завдання 2: Пошук з параметром Func (предикат)
Console.WriteLine("Додаткове 2: Пошук за предикатом (фільтрація залишку > 40)");
var filteredProducts = service.FindBy(p => p.Quantity > 40);
Console.WriteLine($"Знайдено товарів з кількістю > 40: {filteredProducts.Count}");
foreach (var p in filteredProducts)
{
    Console.WriteLine($"-> {p.Name} (Кількість: {p.Quantity} {p.Unit})");
}
Console.WriteLine();

// Додаткове завдання 3: Фабрика створення сховищ StoreFactory
Console.WriteLine("Додаткове 3: Фабрика StoreFactory");
var factoryStore = StoreFactoryCreateDemo(args);
Console.WriteLine($"Фабрика успішно створила сховище типу: {factoryStore.GetType().Name}");
Console.WriteLine("==================================================");

// Допоміжний метод для безпечного виконання операцій і перехоплення винятків
static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"{title}: виняток не спрацював!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{title} -> {ex.GetType().Name}: {ex.Message}");
    }
}

// Імітація методів для демонстрації додаткових завдань
static ICatalogStore StoreFactoryCreateDemo(string[] arguments)
{
    bool fileMode = arguments.Contains("--file");
    string path = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
    return fileMode ? new FileCatalogStore(path) : new InMemoryCatalogStore(SampleData.Products());
}