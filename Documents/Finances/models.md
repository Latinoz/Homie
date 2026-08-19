# Модели данных модуля Finances

## Обзор

Модуль использует 16 моделей данных (+ конфигурационный POCO `FinancesSettings`), организованных в логические группы:

1. **Счета** — AccountModel, BankModel, BrokerModel, WalletModel, CryptoExchangeModel
2. **Инструменты** — InstrumentModel, CurrencyModel, ExchangeRateModel
3. **Позиции** — InvestmentPositionModel, CryptoAssetModel, DepositModel, PreciousMetalModel
4. **Операции** — OperationModel, OperationTypeModel
5. **История** — PriceHistoryModel, InflationModel

---

## Перечисления (Enums)

```csharp
// Тип счёта
public enum AccountType
{
    Bank = 0,           // Банковский счёт
    Broker = 1,         // Брокерский счёт
    Wallet = 2,         // Криптокошелёк
    CryptoExchange = 3  // Криптобиржа
}

// Тип кошелька в справочнике
public enum WalletType
{
    Fiat = 0,           // Обычный (фиатный) кошелёк
    Crypto = 1          // Криптокошелёк
}

// Тип инструмента
public enum InstrumentType
{
    Stock = 0,              // Акция
    Bond = 1,               // Облигация
    ETF = 2,                // Фонд
    Crypto = 3,             // Криптовалюта
    PreciousMetalRef = 4    // Драгоценный металл (ссылка)
}

// Биржа
public enum Exchange
{
    None = 0,       // Не указана
    MOEX = 1,       // Московская биржа
    NYSE = 2,       // Нью-Йоркская биржа
    NASDAQ = 3,     // NASDAQ
    LSE = 4,        // Лондонская биржа
    CryptoSpot = 5  // Криптовалютный рынок
}

// Источник цены
public enum PriceSource
{
    Manual = 0,         // Ручной ввод
    CbrAuto = 1,        // ЦБ РФ (автоматически)
    MoexAuto = 2,       // MOEX (автоматически)
    YahooAuto = 3,      // Yahoo Finance (автоматически)
    CoinGeckoAuto = 4   // CoinGecko (автоматически)
}

// Категория операции
public enum OperationCategory
{
    Deposit = 0,        // Депозитные операции
    Investment = 1,     // Инвестиционные операции
    Crypto = 2,         // Криптовалютные операции
    PreciousMetal = 3,  // Операции с драгметаллами
    Transfer = 4,       // Переводы между счетами
    Other = 5           // Прочие операции
}

// Тип драгоценного металла
public enum MetalType
{
    Gold = 0,       // Золото
    Silver = 1,     // Серебро
    Platinum = 2,   // Платина
    Palladium = 3   // Палладий
}

// Сортировка списков
public enum FinanceSortState
{
    NameAsc, NameDesc,
    DateAsc, DateDesc,
    AmountAsc, AmountDesc,
    CurrencyAsc, CurrencyDesc,
    TypeAsc, TypeDesc
}
```

---

## Модели счетов

### AccountModel

Универсальная модель счёта с полиморфной связью.

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Name` | string | Название счёта (Required, varchar(255)) |
| `AccountType` | AccountType | Тип счёта (Bank/Broker/Wallet/CryptoExchange) |
| `CurrencyId` | int | FK на валюту (базовая валюта счёта) |
| `IsActive` | bool | Активен ли счёт (default: true) |
| `Notes` | string | Заметки (varchar(500)) |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |
| `BankId` | int? | FK на банк (nullable) |
| `BrokerId` | int? | FK на брокера (nullable) |
| `WalletId` | int? | FK на кошелёк (nullable) |
| `CryptoExchangeId` | int? | FK на криптобиржу (nullable) |

**Связи:**
- `Currency` → CurrencyModel — валюта счёта
- `Bank` → BankModel / `Broker` → BrokerModel / `Wallet` → WalletModel / `CryptoExchange` → CryptoExchangeModel — связанная организация (полиморфная, только одно из полей заполнено)
- `Deposits` → ICollection\<DepositModel\> — список вкладов (one-to-many)

---

### BankModel

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Name` | string | Название банка (Required, varchar(255)) |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

---

### BrokerModel

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Name` | string | Название брокера (Required, varchar(255)) |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

---

### WalletModel

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Name` | string | Название кошелька (Required, varchar(255)) |
| `Type` | WalletType | Тип кошелька: Fiat (обычный) / Crypto (криптокошелёк) |
| `CurrencyId` | int? | FK на валюту (nullable) |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

**Связи:**
- `Currency` → CurrencyModel — валюта кошелька (опционально)

**Особенность:** для кошелька с типом `Crypto` автоматически создаётся связанный финансовый счёт (`AccountModel`, `AccountType.Wallet`, `WalletId`), через который криптоактивы и операции журнала привязываются к кошельку.

---

### CryptoExchangeModel

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Name` | string | Название биржи (Required, varchar(255)) |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

---

## Модели инструментов

### CurrencyModel

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ (автоинкремент) |
| `Code` | string | Код валюты (RUB, USD, EUR) (Required, varchar(10)) |
| `Name` | string | Название валюты (Required, varchar(255)) |
| `CbrCode` | string | Код ЦБ РФ для API (varchar(20)) |
| `IsBase` | bool | Базовая валюта (RUB = true) |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

**Связи:**
- `ExchangeRates` — история курсов
- `Instruments` — инструменты в этой валюте
- `Accounts` — счета в этой валюте

---

### InstrumentModel

Универсальная модель финансового инструмента.

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Code` | string | Тикер (SBER, AAPL, BTC) (Required, varchar(50)) |
| `Name` | string | Полное название (Required, varchar(255)) |
| `Type` | InstrumentType | Тип инструмента |
| `Exchange` | Exchange | Биржа |
| `ISIN` | string | Международный код (varchar(20)) |
| `ExternalCode` | string | Внешний код для API (varchar(50)) |
| `CurrencyId` | int | FK на валюту котировки |
| `Category` | string | Категория (varchar(100), опционально) |
| `LastPrice` | decimal? | Последняя цена (decimal(18,6), nullable) |
| `LastPriceDate` | DateTime? | Дата последнего обновления |
| `LastPriceSource` | PriceSource? | Источник последней цены (nullable) |
| `Notes` | string | Заметки (varchar(500)) |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

**Связи:**
- `Currency` → CurrencyModel — валюта инструмента
- `Operations` → операции с инструментом
- `InvestmentPositions` → позиции по инструменту
- `PriceHistory` → история цен

---

### ExchangeRateModel

История курсов валют.

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `CurrencyId` | int | FK на валюту (Required) |
| `Date` | DateTime | Дата курса (Required) |
| `Rate` | decimal | Курс к базовой валюте (Required, decimal(18,6)) |
| `Source` | PriceSource | Источник курса |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

**Связи:**
- `Currency` → CurrencyModel

---

## Модели позиций

### DepositModel

Банковский вклад.

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Name` | string | Название вклада (Required, varchar(255)) |
| `AccountId` | int | FK на счёт |
| `CurrencyId` | int | FK на валюту вклада |
| `Amount` | decimal | Начальная сумма (decimal(18,2)) |
| `InterestRate` | decimal | Процентная ставка, % (decimal(5,2)) |
| `OpenDate` | DateTime | Дата открытия |
| `EndDate` | DateTime? | Дата закрытия (nullable) |
| `IsCapitalization` | bool | Капитализация процентов |
| `Notes` | string | Условия вклада (varchar(500)) |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

**Вычисляемые поля (NotMapped):**
| Поле | Тип | Описание |
|------|-----|----------|
| `BalanceFromJournal` | decimal | Баланс из журнала операций |
| `AccruedInterest` | decimal | Начисленные проценты |
| `ValueInRub` | decimal | Стоимость в рублях |

**Связи:**
- `Account` → AccountModel
- `Currency` → CurrencyModel

---

### InvestmentPositionModel

Инвестиционная позиция (акции, облигации, ETF).

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `InstrumentId` | int | FK на инструмент |
| `AccountId` | int | FK на брокерский счёт |
| `Quantity` | decimal | Количество (decimal(18,6)) |
| `AvgPurchasePrice` | decimal | Средняя цена покупки (decimal(18,6)) |
| `CurrentPrice` | decimal | Текущая цена (decimal(18,6)) |
| `IsAutoUpdateEnabled` | bool | Автообновление цены (default: true) |
| `LastManualOverrideDate` | DateTime? | Дата ручного ввода |
| `Notes` | string | Заметки (varchar(500)) |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

**Вычисляемые поля (NotMapped):**
| Поле | Тип | Описание |
|------|-----|----------|
| `QuantityFromJournal` | decimal | Количество из журнала |
| `AvgPriceFromJournal` | decimal | Средняя цена из журнала |
| `DividendsFromJournal` | decimal | Полученные дивиденды |
| `ReturnPercent` | decimal | Доходность (%) |
| `ValueInRub` | decimal | Стоимость в рублях |

**Связи:**
- `Instrument` → InstrumentModel
- `Account` → AccountModel

---

### CryptoAssetModel

Криптовалютный актив.

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `InstrumentId` | int | FK на инструмент (Required, Range(1, MaxValue)) |
| `AccountId` | int? | FK на счёт, биржа/кошелёк (nullable) |
| `Ticker` | string | Тикер (BTC, ETH) (Required, varchar(20)) |
| `CoinGeckoId` | string | ID для CoinGecko API (varchar(50)) |
| `CurrencyId` | int | FK на валюту учёта (Required, Range(1, MaxValue)) |
| `Quantity` | decimal | Количество (decimal(18,8), 8 знаков после запятой) |
| `AvgPurchasePrice` | decimal | Средняя цена покупки (decimal(18,6)) |
| `CurrentPrice` | decimal | Текущая цена (decimal(18,6)) |
| `IsAutoUpdateEnabled` | bool | Автообновление цены (default: true) |
| `LastManualOverrideDate` | DateTime? | Дата ручного ввода |
| `Notes` | string | Заметки (varchar(500)) |
| `WalletAddress` | string | Адрес кошелька (varchar(255)) |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

**Вычисляемые поля (NotMapped):**
| Поле | Тип | Описание |
|------|-----|----------|
| `QuantityFromJournal` | decimal | Количество из журнала |
| `ValueInUsd` | decimal | Стоимость в USD |
| `ValueInRub` | decimal | Стоимость в RUB |

---

### PreciousMetalModel

Драгоценный металл.

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Metal` | MetalType | Тип металла |
| `Name` | string | Название (слиток, монета) (Required, varchar(255)) |
| `Purity` | string | Проба (999, 585) (varchar(10)) |
| `WeightGrams` | decimal | Вес единицы, грамм (decimal(10,3)) |
| `Quantity` | int | Количество единиц |
| `PurchasePrice` | decimal | Цена покупки за единицу (decimal(18,2)) |
| `PurchaseDate` | DateTime? | Дата покупки (nullable) |
| `CurrentPricePerGram` | decimal | Текущая цена за грамм (decimal(18,2)) |
| `IsAutoUpdateEnabled` | bool | Автообновление цены (default: true) |
| `LastManualOverrideDate` | DateTime? | Дата ручного ввода |
| `Notes` | string | Заметки (varchar(500)) |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

**Вычисляемые поля (NotMapped):**
| Поле | Тип | Описание |
|------|-----|----------|
| `TotalWeightGrams` | decimal | Общий вес = WeightGrams × Quantity |
| `TotalValueRub` | decimal | Общая стоимость = TotalWeightGrams × CurrentPricePerGram |

---

## Модели операций

### OperationModel

Журнал операций (транзакции).

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Date` | DateTime | Дата операции (Required) |
| `OperationTypeId` | int | FK на тип операции |
| `AccountId` | int | FK на счёт |
| `InstrumentId` | int? | FK на инструмент (nullable) |
| `Quantity` | decimal? | Количество (decimal(18,6), nullable) |
| `Price` | decimal? | Цена за единицу (decimal(18,6), nullable) |
| `CurrencyId` | int | FK на валюту |
| `ExchangeRateToRub` | decimal | Курс к рублю на дату (decimal(18,6)) |
| `AmountInRub` | decimal | Сумма в рублях (decimal(18,2)) |
| `Commission` | decimal? | Комиссия (decimal(18,2), nullable) |
| `Tax` | decimal? | Налог (decimal(18,2), nullable) |
| `Notes` | string | Комментарий (varchar(500)) |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

**Связи:**
- `OperationType` → OperationTypeModel — тип операции
- `Account` → AccountModel — счёт
- `Instrument` → InstrumentModel — инструмент (опционально)
- `Currency` → CurrencyModel — валюта

---

### OperationTypeModel

Справочник типов операций.

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Name` | string | Название типа (Required, varchar(100)) |
| `Category` | OperationCategory | Категория |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

**Предустановленные типы (15 шт.):**
- Пополнение вклада, Снятие со вклада, Начисление процентов
- Покупка ценных бумаг, Продажа ценных бумаг
- Получение дивидендов, Получение купонов
- Покупка криптовалюты, Продажа криптовалюты
- Покупка драгметалла, Продажа драгметалла
- Перевод между счетами
- Прочие расходы, Прочие доходы

**Защита от удаления:** тип нельзя удалить, если он используется в операциях.

---

## Модели истории

### PriceHistoryModel

История цен инструментов.

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `InstrumentId` | int | FK на инструмент |
| `Date` | DateTime | Дата (Required) |
| `Price` | decimal | Цена в валюте инструмента (Required, decimal(18,6)) |
| `CurrencyId` | int | FK на валюту |
| `PriceInRub` | decimal | Цена в рублях (decimal(18,2)) |
| `Source` | PriceSource | Источник данных |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

**Связи:**
- `Instrument` → InstrumentModel
- `Currency` → CurrencyModel

---

### InflationModel

Данные об инфляции (ИПЦ).

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Year` | int | Год (Required) |
| `Month` | int | Месяц 1-12 (Required, Range(1,12)) |
| `CpiPercent` | decimal | ИПЦ за месяц, % (decimal(5,2)) |
| `AccumulatedYearPercent` | decimal? | Накопленная инфляция за год, % (decimal(8,4), nullable, автовычисляется) |
| `Notes` | string | Заметки (varchar(500)) |
| `UserUid` | string | Идентификатор пользователя (varchar(255)) |

**Автовычисление:** при создании записи контроллер автоматически рассчитывает `AccumulatedYearPercent` на основе предыдущих месяцев.

---

## ViewModels

### FinanceDashboardViewModel

| Поле | Тип | Описание |
|------|-----|----------|
| `TotalCapital` | decimal | Общий капитал в рублях |
| `TotalDeposits` | decimal | Сумма вкладов |
| `TotalInvestments` | decimal | Стоимость инвестиций |
| `TotalCrypto` | decimal | Стоимость криптовалюты |
| `TotalPreciousMetals` | decimal | Стоимость драгметаллов |
| `TotalReturnPercent` | decimal | Общая доходность портфеля |
| `RealValueAfterInflation` | decimal | Реальная стоимость с учётом инфляции |
| `AccumulatedInflationPercent` | decimal | Накопленная инфляция |
| `AssetAllocationJson` | string | JSON для круговой диаграммы |
| `CapitalDynamicsJson` | string | JSON для графика динамики |
| `LastPriceUpdateDate` | DateTime? | Дата последнего обновления |
| `LatestRates` | List\<ExchangeRateModel\> | Курсы валют |
| `RecentOperations` | List\<OperationModel\> | Последние операции |

### ListViewModels (13 шт.)

- **AccountListViewModel**: Accounts, PageViewModel, CurrentSort, NameFilter, TypeFilter
- **DepositListViewModel**: Deposits, PageViewModel, CurrentSort, NameFilter, AccountFilter
- **InvestmentListViewModel**: Positions, PageViewModel, CurrentSort, NameFilter, TypeFilter
- **CryptoListViewModel**: Assets, PageViewModel, CurrentSort, NameFilter
- **PreciousMetalListViewModel**: Metals, PageViewModel, CurrentSort, MetalFilter, TotalGoldRub, TotalSilverRub, TotalPlatinumRub, TotalPalladiumRub
- **OperationListViewModel**: Operations, PageViewModel, CurrentSort, CategoryFilter, AccountFilter, DateFrom, DateTo
- **InstrumentListViewModel**: Instruments, PageViewModel, CurrentSort, NameFilter, TypeFilter
- **CurrencyListViewModel**: Currencies, LatestRates, PageViewModel
- **BankListViewModel**: Banks, PageViewModel
- **BrokerListViewModel**: Brokers, PageViewModel
- **WalletListViewModel**: Wallets, PageViewModel
- **CryptoExchangeListViewModel**: CryptoExchanges, PageViewModel
- **InflationListViewModel**: Records, PageViewModel, YearFilter

### PriceViewModels (4 шт.)

- **PriceUpdateResultViewModel**: TotalProcessed, SuccessCount, ErrorCount, SkippedCount, Items (List\<PriceUpdateItemResult\>), UpdatedAt
- **PriceUpdateItemResult**: InstrumentName, Ticker, OldPrice, NewPrice, Source, Success, ErrorMessage, Skipped, SkipReason
- **ManualPriceEntryViewModel**: InstrumentId, Date (default: Today), Price, CurrencyId, UpdateCurrentPrice (default: true)
- **PriceHistoryChartViewModel**: InstrumentId, InstrumentName, Ticker, ChartDataJson

---

## Диаграмма связей

```
┌────────────────┐     ┌────────────────┐     ┌────────────────┐
│    Currency    │────<│ ExchangeRate   │     │   Inflation    │
│  (Id = PK)     │     │   (history)    │     │   (monthly)    │
└────────────────┘     └────────────────┘     └────────────────┘
        │
        │ 1:N
        ▼
┌────────────────┐     ┌────────────────┐
│   Instrument   │────<│  PriceHistory  │
│  (Code, Type)  │     │   (daily)      │
└────────────────┘     └────────────────┘
        │
        │ 1:N
        ├─────────────────────────────────┐
        ▼                                 ▼
┌────────────────┐     ┌────────────────┐
│ Investment     │     │  CryptoAsset   │
│ Position       │     │                │
└────────────────┘     └────────────────┘
        │                     │
        └──────────┬──────────┘
                   │
                   ▼ N:1
┌────────────────┐
│    Account     │──────┬──────┬──────┬──────┐
│ (polymorphic)  │      │      │      │      │
└────────────────┘      ▼      ▼      ▼      ▼
        │           ┌──────┐┌──────┐┌──────┐┌────────┐
        │           │ Bank ││Broker││Wallet││CryptoEx│
        │           └──────┘└──────┘└──────┘└────────┘
        │ 1:N
        ▼
┌────────────────┐     ┌────────────────┐
│    Deposit     │     │ PreciousMetal  │
│                │     │                │
└────────────────┘     └────────────────┘
        │                     │
        └──────────┬──────────┘
                   │
                   ▼ N:1
┌────────────────┐     ┌────────────────┐
│   Operation    │────>│ OperationType  │
│   (journal)    │     │  (reference)   │
└────────────────┘     └────────────────┘
```
