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
        // Група маршрутів для продуктів із загальним префіксом /api/products
        var group = app.MapGroup("/api/products").WithTags("Products");

        // 1. Отримання списку продуктів
        // -----------------------------------------------------------------
        // ДОДАТКОВЕ ЗАВДАННЯ №1 (Пагінація та пошук):
        // Сюди додаються параметри запиту: (int? page, int? pageSize, string? search, ICatalogStore store)
        // для фільтрації списку та розбиття на сторінки ({ items, total }).
        // -----------------------------------------------------------------
        group.MapGet("/", (ICatalogStore store) =>
            TypedResults.Ok(store.List().Select(p => p.ToApiDto()).ToList()));

        // 2. Отримання одного продукту за його ідентифікатором (id)
        group.MapGet("/{id}", Results<Ok<ProductDto>, NotFound> (string id, ICatalogStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(found.ToApiDto())
                : TypedResults.NotFound());

        // 3. Створення нового продукту (POST)
        // -----------------------------------------------------------------
        // ДОДАТКОВЕ ЗАВДАННЯ №2 (FluentValidation та AddEndpointFilter):
        // Замість ручного виклику request.TryValidate всередині тіла, 
        // валідація виноситься у FluentValidation і викликається автоматично через фільтр (.AddEndpointFilter<ValidationFilter<SaveProductRequest>>()).
        //
        // ДОДАТКОВЕ ЗАВДАННЯ №3 (DomainException -> 409 Conflict):
        // Якщо доменний метод Product.Create викине кастомний виняток (DomainException),
        // він перехоплюється глобальним обробником у Program.cs і перетворюється на 409 Conflict.
        // -----------------------------------------------------------------
        group.MapPost("/", Results<Created<ProductDto>, ValidationProblem> (SaveProductRequest request, ICatalogStore store) =>
        {
            // Перевірка вхідних даних; при помилці повертається 400 ValidationProblem
            if (!request.TryValidate(out var errors)) return TypedResults.ValidationProblem(errors);

            // Генерація унікального ID та створення доменної сутності
            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var created = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity);
            store.Add(created);

            // Повернення статусу 201 Created разом із заголовком Location
            return TypedResults.Created($"/api/products/{created.Id}", created.ToApiDto());
        });

        // 4. Повне оновлення існуючого продукту (PUT)
        group.MapPut("/{id}", Results<Ok<ProductDto>, NotFound, ValidationProblem> (string id, SaveProductRequest request, ICatalogStore store) =>
        {
            if (!request.TryValidate(out var errors)) return TypedResults.ValidationProblem(errors);
            if (store.GetById(id) is null) return TypedResults.NotFound();

            // Створення заміни з тим самим id та оновлення у сховищі
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