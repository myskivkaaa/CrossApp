using System.IO;
using Core.Domain;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public sealed class FileCatalogStoreTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    [Fact]
    public void FileCatalogStore_AddAndReload_PreservesProducts()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "items.json");
        var store1 = new FileCatalogStore(filePath);
        var product = Product.Create("test-id-1", "SKU-TEST", "Тестовий товар", "шт", 15);

        // Act
        store1.Add(product);

        // Створюємо новий екземпляр сховища для тієї ж папки (перевірка зчитування з диска)
        var store2 = new FileCatalogStore(filePath);
        var items = store2.List();

        // Assert
        var retrieved = Assert.Single(items);
        Assert.Equal(product.Id, retrieved.Id);
        Assert.Equal(product.Sku, retrieved.Sku);
        Assert.Equal(product.Name, retrieved.Name);
        Assert.Equal(product.Quantity, retrieved.Quantity);
    }

    public void Dispose()
    {
        // Очищення тимчасового каталогу після виконання тесту[cite: 9]
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }
}