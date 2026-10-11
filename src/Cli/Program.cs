using Core.Domain;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=== Сценарій 1: успіх ===");
Product product = Product.Create("P-001", "sku-001", "Цемент М400 25кг", "шт", 100);
Console.WriteLine(product);
product.RegisterArrival(50);
product.Issue(30);
Console.WriteLine(product);

Console.WriteLine();
Console.WriteLine("=== Додаткове завдання 3: Перевірка переходів статусів (switch expression) ===");
Console.WriteLine($"Початковий статус: {product.Status}");
product.ChangeStatus(ProductStatus.Active);
Console.WriteLine($"Новий статус після активації: {product.Status}");
TryDo("Недопустимий перехід статусу (з Active назад у Draft)", () => product.ChangeStatus(ProductStatus.Draft));

Console.WriteLine();
Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");
TryDo("видача більша за залишок", () => product.Issue(1000));
TryDo("порожній SKU", () => Product.Create("P-002", "", "Пісок", "т", 10));
TryDo("від'ємний залишок", () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5));

Console.WriteLine();
Console.WriteLine("=== Додаткове завдання 1: Зв'язок імпорту з тижнем 3 та доменними інваріантами ===");
string filePath = Path.Combine("data", "sample.csv");
if (File.Exists(filePath))
{
    var csvResult = ProductCsvImporter.Load(filePath);
    var (validProducts, domainErrors) = DomainImporter.ProcessImport(csvResult);

    Console.WriteLine($"Успішно створено доменних об'єктів: {validProducts.Count}");
    Console.WriteLine($"Відсіяно через помилки: {domainErrors.Count}");
    foreach (var err in domainErrors)
    {
        Console.WriteLine($"  ! {err}");
    }
}
else
{
    Console.WriteLine("Файл sample.csv не знайдено.");
}

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював, інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} - {ex.Message}");
    }
}