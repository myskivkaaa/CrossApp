using System.Collections.Generic;
using Core.Domain;

public static class TestData
{
    public static IReadOnlyList<Product> Items()
    {
        return new List<Product>
        {
            Product.Create("test", "SKU-BIG", "Великий товар", "шт", 100), // 1-й у топі
            Product.Create("ID-2", "SKU-MID", "Середній товар", "шт", 50),  // 2-й у топі
            Product.Create("ID-3", "SKU-LOW1", "Малий товар 1", "шт", 3),  // Менше 10 (дефіцит)
            Product.Create("ID-4", "SKU-LOW2", "Малий товар 2", "шт", 5)   // Менше 10 (дефіцит)
        };
    }
}