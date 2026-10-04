using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class ProductCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<IEntityItem> Load(string path)
    {
        var items = new List<IEntityItem>();
        var errors = new List<string>();
        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (number == 1 && line.StartsWith("id", StringComparison.OrdinalIgnoreCase) || line.StartsWith("type", StringComparison.OrdinalIgnoreCase))
                continue; // пропуск рядка заголовків

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<IEntityItem>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);
        return parts switch
        {
            // Додаткове завдання 2: Рядок із префіксом "P" (Товар)
            ["P", var id, var sku, var name, var unit, var qty] when !int.TryParse(qty, out int q) || q < 0 
                => new ParseFailed($"кількість '{qty}' не є невід'ємним числом"),
            ["P", var id, var sku, var name, var unit, var qty] 
                => new ParseOk(new ProductDto(id, sku, name, unit, int.Parse(qty))),

            // Додаткове завдання 2: Рядок із префіксом "W" (Склад / Warehouse)
            ["W", var id, var code, var location, var manager] 
                => new ParseOk(new WarehouseDto(id, code, location, manager)),

            // Стандартні перевірки для звичайних рядків без префікса (сумісність із базовим sample.csv)
            { Length: < 5 } => new ParseFailed($"очікую 5 колонок, отримав {parts.Length}"),
            ["", _, _, _, _] or [_, "", _, _, _] => new ParseFailed("SKU або назва порожні"),
            [_, _, _, _, var qty] when !int.TryParse(qty, out int q) || q < 0 => new ParseFailed($"кількість '{qty}' не є невід'ємним числом"),
            [var id, var sku, var name, var unit, var qty] => new ParseOk(new ProductDto(id, sku, name, unit, int.Parse(qty))),
            
            _ => new ParseFailed($"невідомий формат або занадто багато колонок: {parts.Length}")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(IEntityItem Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}