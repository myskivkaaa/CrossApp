// 8 лаб + додаткове 1

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Core.Domain;

namespace Core.Abstractions
{
    public interface ICatalogImporter
    {
        IAsyncEnumerable<Product> LoadStreamAsync(string path, CancellationToken ct = default);
    }
}