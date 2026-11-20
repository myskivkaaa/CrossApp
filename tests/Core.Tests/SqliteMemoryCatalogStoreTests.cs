using Core.Domain;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed class SqliteMemoryCatalogStoreTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;

    public SqliteMemoryCatalogStoreTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
    }

    [Fact]
    public void Add_List_Update_Remove_WorksInMemorySqlite()
    {
        var store = new EfCatalogStore(_db);
        var product = Product.Create("prod-1", "SKU-101", "Кабель", "м", 50);

        store.Add(product);
        var all = store.List();
        Assert.Single(all);
        Assert.Equal("SKU-101", all[0].Sku);

        var updatedProduct = Product.Create("prod-1", "SKU-101", "Кабель мідний", "м", 50);
        store.Update(updatedProduct);
        var updated = store.GetById("prod-1");
        Assert.NotNull(updated);
        Assert.Equal("Кабель мідний", updated.Name);

        var removed = store.Remove("prod-1");
        Assert.True(removed);
        Assert.Empty(store.List());
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}