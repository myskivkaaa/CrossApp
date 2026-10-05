namespace Core.Dto;

// Маркерний інтерфейс для підтримки різнорідних сутностей (Додаткове завдання 2)
public interface IEntityItem { }

// Додатковий тип сутності для складів (розпізнається за префіксом 'W')
public record WarehouseDto(
    string Id,
    string Code,
    string Location,
    string Manager) : IEntityItem;


public record ProductDto(
    string Id,
    string Sku,
    string Name,
    string Unit,
    int Quantity,
    string? Note = null) : IEntityItem;