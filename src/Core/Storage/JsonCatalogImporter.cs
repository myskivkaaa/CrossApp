// лаб 8+дод
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage
{
    public sealed class JsonCatalogImporter : ICatalogImporter
    {
        public async IAsyncEnumerable<Product> LoadStreamAsync(
            string path, 
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                FileShare.Read, 4096, useAsync: true);

            var dtos = await JsonSerializer.DeserializeAsync<List<ProductDto>>(
                stream, cancellationToken: ct)
                ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");

            for (var i = 0; i < dtos.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                Product product;
                try 
                { 
                    product = Product.FromDto(dtos[i]); 
                }
                catch (ArgumentException ex)
                { 
                    throw new DomainException($"Запис {i + 1}: {ex.Message}", ex); 
                }
                yield return product;
            }
        }
    }
}