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
│   DbSets (16):            │   │   - Yahoo Finance             │
│   - Currencies            │   │   - CoinGecko                 │
│   - ExchangeRates         │   └───────────────────────────────┘
│   - Instruments           │
│   - OperationTypes        │
│   - Banks                 │
│   - Brokers               │
│   - Wallets               │
│   - CryptoExchanges       │
│   - FinanceAccounts       │
│   - Deposits              │
│   - InvestmentPositions   │
│   - CryptoAssets          │
│   - PreciousMetals        │
│   - FinanceOperations     │
│   - Inflation             │
│   - PriceHistory          │
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
            if (!_settings.EnableBackgroundPriceUpdate)
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                continue;
            }

            // Вычисляет точный delay до следующего целевого времени
            var delay = CalculateDelayUntilNextRun();
            await Task.Delay(delay, stoppingToken);

            await RunUpdateAsync(stoppingToken);
        }
    }

    private async Task RunUpdateAsync(CancellationToken ct)
    {
        // 1. Обновление курсов ЦБ РФ для всех пользователей
        await _cbrService.FetchCurrencyRatesAsync(DateTime.Today);
        // 2. Вызов оркестратора для каждого пользователя
        await _orchestrator.UpdateAllPricesAsync(userId);
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
    public string CbrUpdateTimeUtc { get; set; } = "08:30";
    public string PriceUpdateTimeUtc { get; set; } = "19:00";
    public bool EnableBackgroundPriceUpdate { get; set; } = true;
    public int ManualOverrideLockHours { get; set; } = 24;
    public string[] DefaultCurrencies { get; set; }
    public string[] DefaultMetals { get; set; }
    public CoinGeckoSettings CoinGecko { get; set; } = new();
    public MoexIssSettings MoexIss { get; set; } = new();
    public YahooFinanceSettings YahooFinance { get; set; } = new();
}
```

## Регистрация сервисов

```csharp
// Program.cs

// Конфигурация
builder.Services.Configure<FinancesSettings>(builder.Configuration.GetSection("Finances"));
builder.Services.Configure<CoinGeckoSettings>(builder.Configuration.GetSection("Finances:CoinGecko"));
builder.Services.Configure<MoexIssSettings>(builder.Configuration.GetSection("Finances:MoexIss"));
builder.Services.Configure<YahooFinanceSettings>(builder.Configuration.GetSection("Finances:YahooFinance"));

// HTTP-клиенты (AddHttpClient, не AddScoped)
builder.Services.AddHttpClient<ICbrExchangeRateService, CbrExchangeRateService>();
builder.Services.AddHttpClient<IMoexPriceService, MoexPriceService>();
builder.Services.AddHttpClient<IYahooFinancePriceService, YahooFinancePriceService>();
builder.Services.AddHttpClient<ICoinGeckoPriceService, CoinGeckoPriceService>();

// Scoped-сервисы
builder.Services.AddScoped<IPriceUpdateOrchestrator, PriceUpdateOrchestrator>();
builder.Services.AddScoped<IFinanceCalculationService, FinanceCalculationService>();

// Фоновый сервис
builder.Services.AddHostedService<PriceUpdateHostedService>();
```
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
