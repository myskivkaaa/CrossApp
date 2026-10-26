# CrossApp
Наскрізний проєкт з крос-платформного програмування.

Предметна область: Склад. 
Сутності: Product (товар), StockBatch (партія), Warehouse (склад), Movement (переміщення). 
Призначення: облік залишків товарів по партіях.

## Запуск
dotnet build
dotnet run --project src/Cli

## Середовище
.NET SDK 10.0, Windows x64

## Порівняння розмірів Self-Contained публікацій
- **win-x64:** 153 МБ
- **linux-x64:** 157 МБ

## Структура Solution
CrossApp/
│
├── CrossApp.sln
├── README.md
├── .gitignore
├── data/
│   ├── catalog.json
│   └── sample.csv
└── src/
    ├── Core/        # Бібліотека класів (доменна модель, DTO, імпорт, сховища, сервіси)
    │   ├── Core.csproj
    │   ├── Abstractions/
    │   │   └── ICatalogStore.cs
    │   ├── Domain/
    │   │   └── Product.cs
    │   ├── Dto/
    │   │   ├── ImportResult.cs
    │   │   └── ProductDto.cs
    │   ├── Import/
    │   │   └── ProductCsvImporter.cs
    │   ├── Services/
    │   │   └── CatalogService.cs
    │   ├── Storage/
    │   │   ├── CachingCatalogStore.cs
    │   │   ├── FileCatalogStore.cs
    │   │   ├── InMemoryCatalogStore.cs
    │   │   └── StoreFactory.cs
    │   ├── EnvironmentInfo.cs
    │   └── SampleData.cs
    └── Cli/         # Консольний клієнт (Composition Root, сценарії, демонстрація)
        ├── Cli.csproj
        └── Program.cs


## Порівняння режимів публікації (RID)

| RID | Режим | Розмір publish | Потрібен runtime |
| :--- | :--- | :--- | :--- |
| `win-x64` | self-contained | ~76.85 МБ | ні |
| `win-x64` | framework-dependent | ~0.19 МБ | так (.NET 10) |
| `linux-x64` | self-contained | ~157 МБ | ні |


## Інваріанти доменної моделі (Лабораторна робота 4)
Обов'язковий рядковий ключ: Поля Id та Sku не можуть бути порожніми чи складатися лише з пробілів (ArgumentException).

Коректність кількості: Початковий залишок при створенні та значення для приходу/видачі мають бути невід'ємними / більшими за нуль (ArgumentOutOfRangeException).

Контроль стану складу: Неможливо видати більшу кількість товару, ніж є на поточному залишку (InvalidOperationException).

Переходи статусів: Статус товару може змінюватися лише за допустимими бізнес-правилами через switch expression (з Draft у Active/Discontinued).

## Архітектура сервісного шару та сховищ (Лабораторна робота 5)
Інтерфейс сховища (ICatalogStore): Абстракція над способом зберігання даних (методи List, GetById, Add, Update, Remove, FindBy).

Реалізації сховищ:

InMemoryCatalogStore (робота в оперативній пам'яті на основі Dictionary).

FileCatalogStore (зберігання на диск у форматі JSON з кешуванням та дозавантаженням).

Декоратор (CachingCatalogStore): Огортає інше сховище для оптимізації доступу через локальний кеш у пам'яті.

Сервісний шар (CatalogService): Інкапсулює бізнес-операції та залежить виключно від абстракції (ICatalogStore) через ін'єкцію в конструкторі.

Composition Root: Точка входу в Program.cs, де за допомогою аргументів командного рядка (--file, --cache) або фабрики StoreFactory створюються та зв'язуються конкретні об'єкти.

## Лабораторна робота №6: Доменні події, делегати Action/Func, лямбди

### Реалізований функціонал:
1. **Доменні події у `Core`**: 
   - Створено `enum CatalogChangeKind` (`Added`, `Received`, `Issued`, `Removed`)[cite: 3].
   - Створено клас даних події `CatalogChangedEventArgs` (успадкований від `EventArgs`)[cite: 3].
   - Додано подію `event EventHandler<CatalogChangedEventArgs>? Changed` у `CatalogService` та приватний метод підняття `OnChanged` після успішних операцій[cite: 3, 4].
2. **Підписки в `Cli`**:
   - Підписано локальний метод `OnCatalogChanged`, який виводить події в консоль та записує їх у файл `logs/app.log`[cite: 5].
   - Додано другого підписника у вигляді лямбда-виразу із замиканням над лічильником `changes`[cite: 5].
   - Продемонстровано відписку (`-=`) та сценарій відмови (коли виняток зупиняє підняття події)[cite: 5].
3. **Використання `Func`**:
   - Реалізовано метод `FindAll` (і `FindBy`) із сигнатурою `Func<Product, bool>` для гнучкої фільтрації даних[cite: 4, 5].
4. **Додаткові завдання**:
   - Реалізовано інтерфейс `INotificationSink` з альтернативними реалізаціями `ConsoleSink` та `FileSink`.
   - Додано метод імпорту `ImportProducts` із параметром `Action<string> reportSkipped` для обробки помилок імпорту.

