// 8 лаб

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
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

    [Fact]
    public async Task ImportAsync_WhenTimeoutOccurs_ThrowsOperationCanceled()
    {
        using var userCts = new CancellationTokenSource();
        // Виправляємо перетворення типу, використовуючи TimeSpan.FromMilliseconds
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(10)); 

        await Task.Delay(50); // даємо таймауту спрацювати

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);
        var service = new CatalogService(new InMemoryCatalogStore());

        string tempFile = Path.GetTempFileName();
        try
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => service.ImportAsync(new JsonCatalogImporter(), tempFile, linkedCts.Token));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}