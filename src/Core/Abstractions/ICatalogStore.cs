using Core.Domain;
namespace Core.Abstractions;

public interface ICatalogStore
{
    IReadOnlyList<Product> List();          // Повертає список усіх товарів лише для читання
    Product? GetById(string id);            // Шукає товар за його унікальним ідентифікатором
    void Add(Product item);                 // Додає новий товар до сховища
    void Update(Product item);              // Оновлює інформацію про існуючий товар
    bool Remove(string id);  
    
    // Новий метод пошуку за предикатом (підготовка до тижнів 6-7)
    IReadOnlyList<Product> FindBy(Func<Product, bool> predicate);               // Видаляє товар за ідентифікатором і повертає успіх операції
}
