using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Events; // НОВЕ: підключаємо простір імен для подій
using Core.Services;
using Core.Storage;

// Встановлюємо кодування консолі для коректного відображення українських символів
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Визначаємо режим роботи за допомогою аргументу командного рядка --file та --cache
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

// ==========================================================
// ЛАБОРАТОРНА РОБОТА №6: Доменні події, делегати Action/Func, лямбди
// ==========================================================
Console.WriteLine("==================================================");
Console.WriteLine("ЛАБОРАТОРНА РОБОТА №6: Доменні події та підписки");
Console.WriteLine("==================================================");

// Налаштування лог-файлу для першого підписника
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

// 1. Перший підписник (локальний метод) — виводить у консоль та дописує у файл
void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} залишок {e.QuantityAfter}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

service.Changed += OnCatalogChanged;

// 2. Другий підписник (лямбда-вираз із замиканням над лічильником)
int changes = 0;
service.Changed += (_, _) => changes++;

// Основний хід роботи (Крок 1-5 методички): додавання та зміна стану з подіями
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
service.Receive(created.Id, 25);

Console.WriteLine($"\n[Події] Отримано подій (лічильник лямбди): {changes}");

// Перевірка відписки від першого обробника (Крок 5)
service.Changed -= OnCatalogChanged;
Console.WriteLine("[Події] Відписали консоль/файл від події. Робимо Receive(5)...");

service.Receive(created.Id, 5); // Ця операція оновтить стан, але запису в консоль/файл уже не буде
Console.WriteLine($"[Події] Отримано подій після відписки (лічильник лямбди): {changes}");

// Сценарій відмови (Крок 6): перевіряємо, що при винятку подія не створюється
Console.WriteLine("\n[Події] Сценарій відмови (виняток під час Receive):");
try
{
    service.Receive("invalid-id-999", 10);
}
catch (Exception ex)
{
    Console.WriteLine($"Зловлено очікуваний виняток: {ex.Message}");
    Console.WriteLine($"Лічильник подій залишився незмінним: {changes}");
}

// Використання Func через FindAll (Крок 7)
Console.WriteLine("\n[Події] Демонстрація методу FindAll з делегатом Func:");
var lowStock = service.FindAll(p => p.Quantity < 100);
Console.WriteLine($"Знайдено товарів з кількістю < 100: {lowStock.Count}");
Console.WriteLine("==================================================\n");

// ==========================================================
// ДОДАТКОВІ ЗАВДАННЯ (ЛАБОРАТОРНА РОБОТА №6)
// ==========================================================
Console.WriteLine("==================================================");
Console.WriteLine("ДОДАТКОВІ ЗАВДАННЯ (Лабораторна №6):");
Console.WriteLine("==================================================");

// Додаткове завдання 1: INotificationSink (ConsoleSink та FileSink через конструктор сервісу)
Console.WriteLine("--- Додаткове 1: Патерн Sink замість подій ---");
var extraLogPath = Path.Combine(AppContext.BaseDirectory, "logs", "sink.log");
Core.Notifications.INotificationSink sink = args.Contains("--file-sink") 
    ? new Core.Notifications.FileSink(extraLogPath) 
    : new Core.Notifications.ConsoleSink();

sink.Notify("[Sink] Це сповіщення надіслано через реалізацію INotificationSink.");

// Додаткове завдання 2: Action<string> як параметр методу імпорту
Console.WriteLine("\n--- Додаткове 2: Імпорт товарів з Action<string> (звіт про пропущені) ---");
var rawData = new[]
{
    ("SKU-X1", "Комутатор 8-портовий", "шт", 10),
    ("", "Брак без SKU", "шт", 5),               // Має викликати виняток інваріанту
    ("SKU-X3", "Патч-корд 2м", "шт", -2),          // Від'ємний залишок — виняток
    ("SKU-X4", "Коннектори RJ-45", "пачка", 100)
};

int imported = service.ImportProducts(rawData, skippedMessage =>
{
    Console.WriteLine($"[Імпорт звіту]: {skippedMessage}");
});

Console.WriteLine($"Успішно імпортовано товарів: {imported}");
Console.WriteLine("==================================================");

// ==========================================================
// ВИВЕДЕННЯ ПАТОЧНОГО СПИСКУ ТОВАРІВ У СХОВИЩІ
// ==========================================================
Console.WriteLine("==================================================");
Console.WriteLine("ПОТОЧНИЙ СПИСОК ТОВАРІВ У СХОВИЩІ:");
Console.WriteLine("==================================================");
foreach (var p in service.All())
{
    Console.WriteLine($"{p.Id}  [{p.Sku,-8}]  {p.Name,-20}  -  {p.Quantity,3} {p.Unit}");
}
Console.WriteLine("==================================================\n");

// ==========================================================
// ДЕМОНСТРАЦІЯ ЗБЕРЕЖЕНИХ МЕТОДІВ (Щоб уникнути попереджень CS8321)
// ==========================================================
Console.WriteLine("ПЕРЕВІРКА ДОПОМІЖНИХ МЕТОДІВ З ПОПЕРЕДНІХ ЛАБ:");
TryDo("Демонстрація TryDo", () => { throw new InvalidOperationException("Тестовий виняток"); });
var testFactoryStore = StoreFactoryCreateDemo(args);
Console.WriteLine($"Фабрика успішно перевірена, тип сховища: {testFactoryStore.GetType().Name}");
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