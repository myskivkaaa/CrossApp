namespace Api.Contracts;

public sealed record ProductDto(string Id, string Sku, string Name, string Unit, int Quantity);

public static class ProductExtensions
{
    public static ProductDto ToApiDto(this Core.Domain.Product product) =>
        new(product.Id, product.Sku, product.Name, product.Unit, product.Quantity);
}