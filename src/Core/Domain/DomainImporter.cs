using Core.Dto;
using Core.Import;

namespace Core.Domain;

public static class DomainImporter
{
    // Метод приймає результат роботи парсера з тижня 3 і перевіряє його за правилами домену
    public static (List<Product> Products, List<string> DomainErrors) ProcessImport(ImportResult<ProductDto> csvResult)
    {
        var products = new List<Product>();
        var domainErrors = new List<string>();

        // Переносимо помилки, які знайшов парсер на тижні 3
        foreach (var csvError in csvResult.Errors)
        {
            domainErrors.Add($"CSV помилка: {csvError}");
        }

        // Проганяємо кожен завантажений DTO через створення доменного об'єкта (з перевіркою інваріантів)
        foreach (var dto in csvResult.Items)
        {
            try
            {
                var product = Product.FromDto(dto);
                products.Add(product);
            }
            catch (Exception ex)
            {
                // Якщо порушено доменне правило — ловимо виняток і записуємо у список помилок
                domainErrors.Add($"SKU: {dto.Sku} -> Порушення інваріанту: {ex.Message}");
            }
        }

        return (products, domainErrors);
    }
}