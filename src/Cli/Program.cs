using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Abstractions;
using Core.Configuration;
using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Core.Reports;
using Microsoft.Extensions.Configuration;

// Встановлюємо кодування консолі для коректного відображення українських символів
Console.OutputEncoding = System.Text.Encoding.UTF8;

// --- ЛАНЦЮЖОК КОНФІГУРАЦІЇ (Лабораторна робота № 9) ---

var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{envName}.json", optional: true)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables(prefix: "CROSSAPP_")
    .AddCommandLine(args)
    .Build();

var options = configuration.Get<AppOptions>() ?? new AppOptions();

// ДОДАТКОВЕ ЗАВДАННЯ №1: Валідація налаштувань на старті
if (string.IsNullOrWhiteSpace(options.Storage.DataPath))
{
    Console.WriteLine("Помилка конфігурації: Шлях DataPath не може бути порожнім.");
    Environment.Exit(2);
}

if (!options.Storage.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Помилка конфігурації: Назва файлу (FileName) має закінчуватися на .json.");
    Environment.Exit(2);
}

// ДОДАТКОВЕ ЗАВДАННЯ №2: Підтримка ключа --data-path через командний рядок (перекриває і файл, і env)
var commandLineDataPath = configuration["data-path"] ?? configuration["Storage:DataPath"];
var legacyDataPath = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");

var finalDataPath = !string.IsNullOrWhiteSpace(commandLineDataPath) && commandLineDataPath != options.Storage.DataPath 
    ? commandLineDataPath 
    : (string.IsNullOrWhiteSpace(legacyDataPath) ? options.Storage.DataPath : legacyDataPath);

var storageOptions = new StorageOptions
{
    DataPath = finalDataPath,
    FileName = options.Storage.FileName
};

var dataPath = storageOptions.ResolveFullPath(AppContext.BaseDirectory);

// Зчитування секрету
var apiKey = configuration["Integration:ApiKey"];
Console.WriteLine($"Файл даних: {dataPath}");
Console.WriteLine(apiKey is null ? "ApiKey: не заданий" : $"ApiKey: присутній, довжина {apiKey.Length}");
Console.WriteLine("==================================================");

// Налаштування кооперативного скасування через Ctrl+C та таймаут
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, timeoutCts.Token);

// Визначаємо режим роботи за допомогою аргументів командного рядка --file та --cache
bool useFile = args.Contains("--file") || args.Contains("export");
bool useCache = args.Contains("--cache");

// Composition Root: ініціалізація сховища з урахуванням конфігурації
ICatalogStore store = useFile
    ? (useCache ? new CachingCatalogStore(new FileCatalogStore(dataPath)) : new FileCatalogStore(dataPath))
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);
var items = service.All();

var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "catalog";

switch (command)
{
    case "export":
        var format = storageOptions.Format.ToLowerInvariant();
        if (format == "csv")
        {
            // Експорт у CSV формат
            var csvPath = Path.ChangeExtension(dataPath, ".csv");
            var csvLines = new List<string> { "Id,Sku,Name,Unit,Quantity" };
            csvLines.AddRange(service.All().Select(p => $"{p.Id},{p.Sku},{p.Name},{p.Unit},{p.Quantity}"));
            File.WriteAllLines(csvPath, csvLines);
            Console.WriteLine($"Експортовано записів (CSV) до {csvPath}: {service.All().Count}");
        }
        else
        {
            // Стандартний JSON експорт (використовує файл сховища)
            Console.WriteLine($"Експортовано записів (JSON): {service.All().Count}");
        }
        break;

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
        
        try
        {
            var progress = new Progress<int>(percent =>
            {
                Console.Write($"\rПрогрес імпорту: {percent}%   ");
            });

            var added = await service.ImportAsync(new JsonCatalogImporter(), dataPath, linkedCts.Token, progress);
            Console.WriteLine();
            Console.WriteLine($"Імпортовано записів: {added}");
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            Console.WriteLine("\nІмпорт перервано за тайм-аутом.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nІмпорт скасовано користувачем.");
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"\nДані відхилено: {ex.Message}");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"\nФайл не знайдено: {dataPath}");
        }

        Console.WriteLine("==================================================");
        Console.WriteLine("ПОТОЧНИЙ СПИСОК ТОВАРІВ У СХОВИЩІ:");
        Console.WriteLine("==================================================");
        foreach (var p in service.All())
        {
            Console.WriteLine($"{p.Id}  [{p.Sku,-8}]  {p.Name,-20}  -  {p.Quantity,3} {p.Unit}");
        }
        Console.WriteLine("==================================================");
        Console.WriteLine("Доступні підкоманди: export | report-top | report-low | report-groups | report-sum | demo-deferred | demo-perf | catalog");
        break;
}

static void PrintStockLines(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
    {
        Console.WriteLine($"{l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
    }
}

static void RunDeferredExecutionDemo()
{
    Console.WriteLine("--- Демонстрація відкладеного виконання (Deferred Execution) ---");
    var sourceList = SampleData.Products().ToList();
    var query = sourceList.Where(p => p.Quantity < 10);
    Console.WriteLine($"Дефіцитних до змін: {query.Count()}");
    sourceList.Add(Product.Create("TMP-001", "SKU-999", "Тестова позиція (дефіцит)", "шт", 3));
    Console.WriteLine($"Дефіцитних після змін: {query.Count()}");
    var snapshot = query.ToList();
    sourceList.Clear();
    Console.WriteLine($"У знімку лишилось (після очищення джерела): {snapshot.Count}");
}

static void RunPerformanceTest()
{
    Console.WriteLine("--- Вимірювання продуктивності (Stopwatch на 100 000 записів) ---");
    var bigList = Enumerable.Range(1, 100_000)
        .Select(i => Product.Create($"P-{i}", $"SKU-{i}", $"Товар {i}", i % 2 == 0 ? "шт" : "м", i % 50))
        .ToList();

    var sw = Stopwatch.StartNew();
    sw.Restart();
    var queryWithoutList = bigList.Where(p => p.Quantity < 20).OrderBy(p => p.Quantity);
    var count1 = queryWithoutList.Count();
    sw.Stop();
    Console.WriteLine($"Без проміжного ToList: {sw.ElapsedMilliseconds} мс (знайдено: {count1})");

    sw.Restart();
    var queryWithList = bigList.Where(p => p.Quantity < 20).ToList().OrderBy(p => p.Quantity);
    var count2 = queryWithList.Count();
    sw.Stop();
    Console.WriteLine($"З проміжним ToList:    {sw.ElapsedMilliseconds} мс (знайдено: {count2})");
}