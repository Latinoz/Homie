# Архитектура модуля Finances

## Структура папок

```
Homie/Areas/Finances/
├── Controllers/                 # Контроллеры (16 шт.)
│   ├── AccountsController.cs
│   ├── BanksController.cs
│   ├── BrokersController.cs
│   ├── CryptoController.cs
│   ├── CryptoExchangesController.cs
│   ├── CurrenciesController.cs
│   ├── DashboardController.cs
│   ├── DepositsController.cs
│   ├── InflationController.cs
│   ├── InstrumentsController.cs
│   ├── InvestmentsController.cs
│   ├── OperationsController.cs
│   ├── OperationTypesController.cs
│   ├── PreciousMetalsController.cs
│   ├── PriceUpdateController.cs
│   └── WalletsController.cs
│
├── Models/                      # Модели данных
│   ├── AccountModel.cs
│   ├── BankModel.cs
│   ├── BrokerModel.cs
│   ├── CryptoAssetModel.cs
│   ├── CryptoExchangeModel.cs
│   ├── CurrencyModel.cs
│   ├── DepositModel.cs
│   ├── Enums.cs
│   ├── ExchangeRateModel.cs
│   ├── FinancesSettings.cs
│   ├── InflationModel.cs
│   ├── InstrumentModel.cs
│   ├── InvestmentPositionModel.cs
│   ├── OperationModel.cs
│   ├── OperationTypeModel.cs
│   ├── PreciousMetalModel.cs
│   ├── PriceHistoryModel.cs
│   ├── WalletModel.cs
│   └── ViewModels/              # Модели представлений
│
├── Services/                    # Сервисы
│   ├── FinanceCalculationService.cs
│   ├── CbrExchangeRateService.cs
│   ├── MoexPriceService.cs
│   ├── YahooFinancePriceService.cs
│   ├── CoinGeckoPriceService.cs
│   ├── PriceUpdateOrchestrator.cs
│   ├── PriceUpdateHostedService.cs
│   └── Interfaces (I*.cs)
│
└── Views/                       # Представления
    ├── Accounts/
    ├── Banks/
    ├── Brokers/
    ├── Crypto/
    ├── CryptoExchanges/
    ├── Currencies/
    ├── Dashboard/
    ├── Deposits/
    ├── Inflation/
    ├── Instruments/
    ├── Investments/
    ├── Operations/
    ├── OperationTypes/
    ├── PreciousMetals/
    ├── PriceUpdate/
    └── Wallets/
```

## Паттерны проектирования

### 1. Repository Pattern

Используется Entity Framework Core DbContext как репозиторий для доступа к данным.

```csharp
// Пример использования в контроллере
private readonly ApplicationDbContext _context;

public async Task<IActionResult> Index()
{
    var deposits = await _context.Deposits
        .Include(d => d.Account)
        .Include(d => d.Currency)
        .Where(d => d.UserUid == _userUid)
        .ToListAsync();
    return View(deposits);
}
```

### 2. Полиморфные счета (Polymorphic Accounts)

`AccountModel` использует nullable foreign keys для связи с разными типами счетов:

```csharp
public class AccountModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public AccountType AccountType { get; set; }
    
    // Полиморфная связь — только одно из полей заполнено
    public int? BankId { get; set; }
    public int? BrokerId { get; set; }
    public int? WalletId { get; set; }
    public int? CryptoExchangeId { get; set; }
    
    public BankModel Bank { get; set; }
    public BrokerModel Broker { get; set; }
    public WalletModel Wallet { get; set; }
    public CryptoExchangeModel CryptoExchange { get; set; }
}
```

### 3. Journal-based Calculations

Позиции рассчитываются на основе журнала операций, а не хранятся напрямую:

```csharp
public class InvestmentPositionModel
{
    // Хранимые поля
    public decimal Quantity { get; set; }
    public decimal AvgPurchasePrice { get; set; }
    
    // Вычисляемые из журнала операций (NotMapped)
    [NotMapped]
    public decimal QuantityFromJournal { get; set; }
    
    [NotMapped]
    public decimal AvgPriceFromJournal { get; set; }
    
    [NotMapped]
    public decimal DividendsFromJournal { get; set; }
}
```

### 4. Multi-tenancy

Все сущности содержат поле `UserUid` для изоляции данных пользователей:

```csharp
public class DepositModel
{
    public int Id { get; set; }
    public string UserUid { get; set; }  // Идентификатор пользователя
    // ...
}
```

### 5. Strategy Pattern для источников цен

Множество сервисов для получения котировок, объединённых оркестратором:

```
IPriceUpdateOrchestrator
    ├── ICbrExchangeRateService     (курсы ЦБ РФ, металлы)
    ├── IMoexPriceService           (акции/облигации MOEX)
    ├── IYahooFinancePriceService   (международные биржи)
    └── ICoinGeckoPriceService      (криптовалюты)
```

## Поток данных

```
┌─────────────────────────────────────────────────────────────────┐
│                         ПОЛЬЗОВАТЕЛЬ                            │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                         КОНТРОЛЛЕРЫ                             │
│   Dashboard, Accounts, Deposits, Investments, Crypto, etc.      │
└─────────────────────────────────────────────────────────────────┘
                                │
                    ┌───────────┴───────────┐
                    ▼                       ▼
┌───────────────────────────┐   ┌───────────────────────────────┐
│   FinanceCalculationService│   │   PriceUpdateOrchestrator     │
│   - Расчёт баланса        │   │   - Обновление котировок      │
│   - Расчёт доходности     │   │   - Координация API-сервисов  │
│   - Данные для дашборда   │   │                               │
└───────────────────────────┘   └───────────────────────────────┘
                    │                       │
                    ▼                       ▼
┌───────────────────────────┐   ┌───────────────────────────────┐
│   ApplicationDbContext    │   │   Внешние API                 │
│   (Entity Framework)      │   │   - ЦБ РФ                     │
│                           │   │   - MOEX ISS                  │
│   Таблицы:                │   │   - Yahoo Finance             │
│   - Accounts              │   │   - CoinGecko                 │
│   - Deposits              │   └───────────────────────────────┘
│   - InvestmentPositions   │
│   - CryptoAssets          │
│   - Operations            │
│   - ExchangeRates         │
│   - PriceHistory          │
│   - ...                   │
└───────────────────────────┘
```

## Фоновые процессы

### PriceUpdateHostedService

Автоматическое обновление котировок по расписанию:

```csharp
public class PriceUpdateHostedService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            
            // Обновление курсов ЦБ РФ (08:30 UTC)
            if (now.TimeOfDay >= _settings.CbrUpdateTimeUtc)
            {
                await _cbrService.FetchCurrencyRatesAsync(DateTime.Today);
            }
            
            // Обновление котировок (19:00 UTC)
            if (now.TimeOfDay >= _settings.PriceUpdateTimeUtc)
            {
                await _orchestrator.UpdateAllPricesAsync(userId);
            }
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

## Конфигурация

### appsettings.json

```json
{
  "Finances": {
    "BaseCurrency": "RUB",
    "CbrUpdateTimeUtc": "08:30",
    "PriceUpdateTimeUtc": "19:00",
    "EnableBackgroundPriceUpdate": true,
    "ManualOverrideLockHours": 24,
    "DefaultCurrencies": ["RUB", "USD", "EUR", "CNY", "GBP"],
    "DefaultMetals": ["Gold", "Silver", "Platinum", "Palladium"],
    "CoinGecko": {
      "BaseUrl": "https://api.coingecko.com/api/v3",
      "RequestDelayMs": 2100
    },
    "MoexIss": {
      "BaseUrl": "https://iss.moex.com/iss"
    },
    "YahooFinance": {
      "BaseUrl": "https://query1.finance.yahoo.com/v8/finance/chart",
      "RequestDelayMs": 1000
    }
  }
}
```

### FinancesSettings.cs

```csharp
public class FinancesSettings
{
    public string BaseCurrency { get; set; } = "RUB";
    public TimeSpan CbrUpdateTimeUtc { get; set; }
    public TimeSpan PriceUpdateTimeUtc { get; set; }
    public bool EnableBackgroundPriceUpdate { get; set; }
    public int ManualOverrideLockHours { get; set; } = 24;
    public List<string> DefaultCurrencies { get; set; }
    public List<string> DefaultMetals { get; set; }
    public CoinGeckoSettings CoinGecko { get; set; }
    public MoexSettings MoexIss { get; set; }
    public YahooFinanceSettings YahooFinance { get; set; }
}
```

## Регистрация сервисов

```csharp
// Program.cs или Startup.cs
services.Configure<FinancesSettings>(configuration.GetSection("Finances"));

services.AddScoped<IFinanceCalculationService, FinanceCalculationService>();
services.AddScoped<ICbrExchangeRateService, CbrExchangeRateService>();
services.AddScoped<IMoexPriceService, MoexPriceService>();
services.AddScoped<IYahooFinancePriceService, YahooFinancePriceService>();
services.AddScoped<ICoinGeckoPriceService, CoinGeckoPriceService>();
services.AddScoped<IPriceUpdateOrchestrator, PriceUpdateOrchestrator>();

services.AddHostedService<PriceUpdateHostedService>();
```

## Диаграмма связей между моделями

```
                    ┌─────────────┐
                    │  Currency   │
                    └─────────────┘
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
        ▼                 ▼                 ▼
┌───────────────┐ ┌───────────────┐ ┌───────────────┐
│    Account    │ │  Instrument   │ │ ExchangeRate  │
└───────────────┘ └───────────────┘ └───────────────┘
        │                 │
        │    ┌────────────┼────────────┐
        │    │            │            │
        ▼    ▼            ▼            ▼
┌─────────────────┐ ┌───────────┐ ┌────────────┐
│    Deposit      │ │Investment │ │CryptoAsset │
└─────────────────┘ │ Position  │ └────────────┘
                    └───────────┘
        │                 │            │
        └─────────────────┼────────────┘
                          ▼
                  ┌───────────────┐
                  │   Operation   │
                  │   (Journal)   │
                  └───────────────┘
```
