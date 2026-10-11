using Core.Dto;
using Core.Import;

namespace Core.Domain;

public static class DomainImporter
{
    public static (List<Product> Products, List<string> DomainErrors) ProcessImport(ImportResult<ProductDto> csvResult)
    {
        var products = new List<Product>();
        var domainErrors = new List<string>();

        foreach (var csvError in csvResult.Errors)
        {
            domainErrors.Add($"CSV помилка: {csvError}");
        }

        foreach (var dto in csvResult.Items)
        {
            try
            {
                var product = Product.FromDto(dto);
                products.Add(product);
            }
            catch (Exception ex)
            {
                domainErrors.Add($"SKU: {dto.Sku} -> Порушення інваріанту: {ex.Message}");
            }
        }

        return (products, domainErrors);
    }
}