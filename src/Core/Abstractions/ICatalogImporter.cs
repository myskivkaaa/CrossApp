// 8 лаб
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain; // Замініть на ваш простір імен для моделей (наприклад, Product)

namespace Core.Abstractions
{
    public interface ICatalogImporter
    {
        Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default);
    }
}