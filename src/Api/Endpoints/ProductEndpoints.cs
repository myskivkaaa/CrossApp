using System.Security.Cryptography;
using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        // 1. Отримання списку продуктів з пошуком і сортуванням
        group.MapGet("/", (string? search, string? sortBy, bool? desc, ICatalogStore store) =>
        {
            var query = store.List().AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p =>
                    p.Sku.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    p.Unit.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var isDesc = desc ?? false;
            query = (sortBy?.ToLowerInvariant()) switch
            {
                "sku" => isDesc ? query.OrderByDescending(p => p.Sku) : query.OrderBy(p => p.Sku),
                "name" => isDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "quantity" => isDesc ? query.OrderByDescending(p => p.Quantity) : query.OrderBy(p => p.Quantity),
                _ => isDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
            };

            return TypedResults.Ok(query.Select(p => p.ToApiDto()).ToList());
        });

        // 2. Отримання одного продукту за його ідентифікатором (id)
        group.MapGet("/{id}", Results<Ok<ProductDto>, NotFound> (string id, ICatalogStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(found.ToApiDto())
                : TypedResults.NotFound());

        // 3. Створення нового продукту (POST)
        group.MapPost("/", Results<Created<ProductDto>, ValidationProblem> (SaveProductRequest request, ICatalogStore store) =>
        {
            if (!request.TryValidate(out var errors)) return TypedResults.ValidationProblem(errors);

            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var created = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
            store.Add(created);

            return TypedResults.Created($"/api/products/{created.Id}", created.ToApiDto());
        });

        // 4. Повне оновлення існуючого продукту (PUT)
        group.MapPut("/{id}", Results<Ok<ProductDto>, NotFound, ValidationProblem> (string id, SaveProductRequest request, ICatalogStore store) =>
        {
            if (!request.TryValidate(out var errors)) return TypedResults.ValidationProblem(errors);
            if (store.GetById(id) is null) return TypedResults.NotFound();

            var replacement = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
            store.Update(replacement);

            return TypedResults.Ok(replacement.ToApiDto());
        });

        // 5. Видалення продукту за id (DELETE)
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, ICatalogStore store) =>
            store.Remove(id)
                ? TypedResults.NoContent()
                : TypedResults.NotFound());
    }
}