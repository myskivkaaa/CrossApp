using Core.Abstractions;

namespace Core.Storage;

public static class StoreFactory
{
    // Статичний метод фабрики для створення екземпляра сховища на основі переданих аргументів командного рядка
    public static ICatalogStore Create(string[] args)
    {
        // Перевіряємо наявність прапорців у масиві аргументів
        bool useFile = args.Contains("--file");
        bool useCache = args.Contains("--cache"); // Дозволяє комбінувати файлове сховище з декоратором кешування
        
        // Формуємо безпечний шлях до файлу даних у папці програми
        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

        // Вибираємо базову реалізацію сховища: файлове або в пам'яті з початковими даними
        ICatalogStore store = useFile
            ? new FileCatalogStore(dataPath)
            : new InMemoryCatalogStore(SampleData.Products());

        // Якщо увімкнено кешування разом із файловим режимом, огортаємо сховище в декоратор
        if (useCache && useFile)
        {
            store = new CachingCatalogStore(store);
        }

        // Повертаємо налаштований екземпляр, що реалізує інтерфейс ICatalogStore
        return store;
    }
}