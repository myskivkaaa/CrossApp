using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Core.Reports;

// Встановлюємо кодування консолі для коректного відображення українських символів
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Визначаємо режим роботи за допомогою аргументів командного рядка --file та --cache
bool useFile = args.Contains("--file");
bool useCache = args.Contains("--cache");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

// Composition Root: єдине місце в додатку, де обирається конкретна реалізація сховища
ICatalogStore store = useFile
    ? (useCache ? new CachingCatalogStore(new FileCatalogStore(dataPath)) : new FileCatalogStore(dataPath))
    : new InMemoryCatalogStore(SampleData.Products());

// Впроваджуємо сховище в бізнес-сервіс через конструктор (Dependency Injection)
var service = new CatalogService(store);

// Отримуємо поточний знімок елементів зі сховища для передачі у чисті функції звітів
var items = service.All();

// Визначаємо підкоманду звітів із аргументів командного рядка (за замовчуванням — catalog)
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "catalog";

switch (command)
{
    case "report-top":
        Console.WriteLine("==================================================");
        Console.WriteLine("ЗВІТ: ТОП-5 позицій за кількістю");
        Console.WriteLine("==================================================");
        PrintStockLines(ReportService.TopByQuantity(items, 5));
        break;

    case "report-low":
        Console.WriteLine("==================================================");
        Console.WriteLine("ЗВІТ: Дефіцитні товари (залишок < 10)");
        Console.WriteLine("==================================================");
        PrintStockLines(ReportService.LowStock(items, 10));
        break;

    case "report-groups":
        Console.WriteLine("==================================================");
        Console.WriteLine("ЗВІТ: Групування за одиницями виміру");
        Console.WriteLine("==================================================");
        foreach (var g in ReportService.ByUnit(items))
        {
            Console.WriteLine($"{g.Key,-6} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
        }
        break;

    case "report-sum":
        Console.WriteLine("==================================================");
        Console.WriteLine("ЗВІТ: Підсумкові агрегати");
        Console.WriteLine("==================================================");
        var t = ReportService.Totals(items);
        Console.WriteLine($"позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
        break;

    case "demo-deferred":
        RunDeferredExecutionDemo();
        break;

    case "demo-perf":
        RunPerformanceTest();
        break;

    case "catalog":
    default:
        Console.WriteLine("==================================================");
        Console.WriteLine($"ПРОЄКТ КАТАЛОГУ (Сховище: {store.GetType().Name})");
        Console.WriteLine("==================================================");
        Console.WriteLine("ПОТОЧНИЙ СПИСОК ТОВАРІВ У СХОВИЩІ:");
        Console.WriteLine("==================================================");
        foreach (var p in service.All())
        {
            Console.WriteLine($"{p.Id}  [{p.Sku,-8}]  {p.Name,-20}  -  {p.Quantity,3} {p.Unit}");
        }
        Console.WriteLine("==================================================");
        Console.WriteLine("Доступні підкоманди: report-top | report-low | report-groups | report-sum | demo-deferred | demo-perf | catalog");
        break;
}

// ==========================================================
// ДОПОМІЖНІ МЕТОДИ ДЛЯ ВИВЕДЕННЯ ТА ДЕМОНСТРАЦІЇ
// ==========================================================

static void PrintStockLines(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
    {
        Console.WriteLine($"{l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
    }
}

// Демонстрація відкладеного виконання (Крок 3 методички)
static void RunDeferredExecutionDemo()
{
    Console.WriteLine("--- Демонстрація відкладеного виконання (Deferred Execution) ---");
    var sourceList = SampleData.Products().ToList();
    
    var query = sourceList.Where(p => p.Quantity < 10); // Запит побудовано, але ще не виконано
    
    Console.WriteLine($"Дефіцитних до змін: {query.Count()}");
    
    sourceList.Add(Product.Create("TMP-001", "SKU-999", "Тестова позиція (дефіцит)", "шт", 3));
    
    Console.WriteLine($"Дефіцитних після змін: {query.Count()}"); // Число змінилося через відкладене виконання
    
    var snapshot = query.ToList(); // Матеріалізація запиту
    sourceList.Clear();
    
    Console.WriteLine($"У знімку лишилось (після очищення джерела): {snapshot.Count}"); // Результат зафіксовано
}

// Додатковий метод вимірювання часу (Додаткове завдання №4)
static void RunPerformanceTest()
{
    Console.WriteLine("--- Вимірювання продуктивності (Stopwatch на 100 000 записів) ---");
    var bigList = Enumerable.Range(1, 100_000)
        .Select(i => Product.Create($"P-{i}", $"SKU-{i}", $"Товар {i}", i % 2 == 0 ? "шт" : "м", i % 50))
        .ToList();

    var sw = Stopwatch.StartNew();
    
    // Варіант А: Без проміжного ToList() (чисте відкладене виконання)
    sw.Restart();
    var queryWithoutList = bigList.Where(p => p.Quantity < 20).OrderBy(p => p.Quantity);
    var count1 = queryWithoutList.Count();
    sw.Stop();
    Console.WriteLine($"Без проміжного ToList: {sw.ElapsedMilliseconds} мс (знайдено: {count1})");

    // Варіант Б: З проміжним ToList() усередині ланцюжка (буферизація)
    sw.Restart();
    var queryWithList = bigList.Where(p => p.Quantity < 20).ToList().OrderBy(p => p.Quantity);
    var count2 = queryWithList.Count();
    sw.Stop();
    Console.WriteLine($"З проміжним ToList:    {sw.ElapsedMilliseconds} мс (знайдено: {count2})");
}