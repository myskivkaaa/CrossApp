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
```text
CrossApp/
│
├── CrossApp.sln
├── README.md
├── .gitignore
└── src/
    ├── Core/        # Бібліотека класів (логіка збору середовища, доменна модель)
    │   ├── Core.csproj
    │   └── EnvironmentInfo.cs
    └── Cli/         # Консольний клієнт (точка входу, форматування виводу)
        ├── Cli.csproj
        └── Program.cs



## Порівняння режимів публікації (RID)

| RID | Режим | Розмір publish | Потрібен runtime |
| :--- | :--- | :--- | :--- |
| `win-x64` | self-contained | ~76.85 МБ | ні |
| `win-x64` | framework-dependent | ~0.19 МБ | так (.NET 10) |
| `linux-x64` | self-contained | ~157 МБ | ні |


## Зміни для здачі лабораторної