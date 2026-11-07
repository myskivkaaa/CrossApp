// 8 лаб

using Xunit;
using System.Threading;
using System.Threading.Tasks;
using Core.Services;
using Core.Storage;

public class ImportTests
{
        [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var service = new CatalogService(new InMemoryCatalogStore());
        string tempFile = Path.GetTempFileName(); // Створюємо реальний тимчасовий файл

        try
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => service.ImportAsync(new JsonCatalogImporter(), tempFile, cts.Token));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile); // Очищаємо за собою
        }
    }
}