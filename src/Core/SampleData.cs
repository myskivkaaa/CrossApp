using System.Collections.Generic;

namespace Core;

public static class SampleData
{
    public static List<Domain.Product> Products() =>
    [
        Domain.Product.Create("P-001", "SKU-001", "Монітор 24\"", "шт", 12),
        Domain.Product.Create("P-002", "SKU-002", "Клавіатура", "шт", 5), // дефіцит
        Domain.Product.Create("P-003", "SKU-003", "Миша", "шт", 8),    // дефіцит
        Domain.Product.Create("P-004", "SKU-004", "Кабель HDMI", "м", 45),
        Domain.Product.Create("P-005", "SKU-005", "Кабель UTP", "м", 120),
        Domain.Product.Create("P-006", "SKU-006", "Комутатор", "шт", 3), // дефіцит
        Domain.Product.Create("P-007", "SKU-007", "Роутер", "шт", 15),
        Domain.Product.Create("P-008", "SKU-008", "Пам'ять DDR4", "шт", 22),
        Domain.Product.Create("P-009", "SKU-009", "SSD Диск", "шт", 14),
        Domain.Product.Create("P-010", "SKU-010", "Термопаста", "тюб", 30),
        Domain.Product.Create("P-011", "SKU-011", "Викрутка", "шт", 7),  // дефіцит
        Domain.Product.Create("P-012", "SKU-012", "Набір стяжок", "пач", 50),
        Domain.Product.Create("P-013", "SKU-013", "Корпус ПК", "шт", 4), // дефіцит
        Domain.Product.Create("P-014", "SKU-014", "Блок живлення", "шт", 9), // дефіцит
        Domain.Product.Create("P-015", "SKU-015", "Припій", "м", 80),
        Domain.Product.Create("P-016", "SKU-016", "Флюс", "тюб", 25),
        Domain.Product.Create("P-017", "SKU-017", "Дисплейний модуль", "шт", 6), // дефіцит
        Domain.Product.Create("P-018", "SKU-018", "Адаптер живлення", "шт", 18)
    ];
}