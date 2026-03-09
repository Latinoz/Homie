# Модели данных модуля Finances

## Обзор

Модуль использует 17 моделей данных, организованных в логические группы:

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
    None,       // Не указана
    MOEX,       // Московская биржа
    NYSE,       // Нью-Йоркская биржа
    NASDAQ,     // NASDAQ
    LSE,        // Лондонская биржа
    CryptoSpot  // Криптовалютный рынок
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
| `Name` | string | Название счёта |
| `AccountType` | AccountType | Тип счёта (Bank/Broker/Wallet/CryptoExchange) |
| `CurrencyId` | int | Базовая валюта счёта |
| `IsActive` | bool | Активен ли счёт |
| `Notes` | string | Заметки |
| `UserUid` | string | Идентификатор пользователя |
| `BankId` | int? | FK на банк (nullable) |
| `BrokerId` | int? | FK на брокера (nullable) |
| `WalletId` | int? | FK на кошелёк (nullable) |
| `CryptoExchangeId` | int? | FK на криптобиржу (nullable) |

**Связи:**
- `Currency` — валюта счёта
- `Bank` / `Broker` / `Wallet` / `CryptoExchange` — связанная организация
- `Deposits` — список вкладов (one-to-many)

---

### BankModel

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Name` | string | Название банка |
| `UserUid` | string | Идентификатор пользователя |

---

### BrokerModel

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Name` | string | Название брокера |
| `UserUid` | string | Идентификатор пользователя |

---

### WalletModel

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Name` | string | Название кошелька |
| `CurrencyId` | int | Основная валюта |
| `UserUid` | string | Идентификатор пользователя |

**Связи:**
- `Currency` — валюта кошелька

---

### CryptoExchangeModel

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Name` | string | Название биржи |
| `UserUid` | string | Идентификатор пользователя |

---

## Модели инструментов

### CurrencyModel

| Поле | Тип | Описание |
|------|-----|----------|
| `Code` | string | Код валюты (RUB, USD, EUR) — PK |
| `Name` | string | Название валюты |
| `CbrCode` | string | Код ЦБ РФ для API |
| `IsBase` | bool | Базовая валюта (RUB = true) |
| `UserUid` | string | Идентификатор пользователя |

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
| `Code` | string | Тикер (SBER, AAPL, BTC) |
| `Name` | string | Полное название |
| `Type` | InstrumentType | Тип инструмента |
| `Exchange` | Exchange | Биржа |
| `ISIN` | string | Международный код (для акций/облигаций) |
| `ExternalCode` | string | Внешний код для API |
| `CurrencyId` | int | Валюта котировки |
| `Category` | string | Категория (опционально) |
| `LastPrice` | decimal | Последняя цена |
| `LastPriceDate` | DateTime? | Дата последнего обновления |
| `LastPriceSource` | PriceSource | Источник последней цены |
| `UserUid` | string | Идентификатор пользователя |

**Связи:**
- `Currency` — валюта инструмента
- `Operations` — операции с инструментом
- `InvestmentPositions` — позиции по инструменту
- `PriceHistory` — история цен

---

### ExchangeRateModel

История курсов валют.

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `CurrencyId` | string | FK на валюту |
| `Date` | DateTime | Дата курса |
| `Rate` | decimal(18,6) | Курс к базовой валюте |
| `Source` | PriceSource | Источник курса |
| `UserUid` | string | Идентификатор пользователя |

---

## Модели позиций

### DepositModel

Банковский вклад.

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Name` | string | Название вклада |
| `AccountId` | int | FK на счёт |
| `CurrencyId` | string | Валюта вклада |
| `Amount` | decimal | Начальная сумма |
| `InterestRate` | decimal(5,2) | Процентная ставка (%) |
| `OpenDate` | DateTime | Дата открытия |
| `EndDate` | DateTime? | Дата закрытия |
| `IsCapitalization` | bool | Капитализация процентов |
| `Notes` | string | Условия вклада |
| `UserUid` | string | Идентификатор пользователя |

**Вычисляемые поля (NotMapped):**
| Поле | Тип | Описание |
|------|-----|----------|
| `BalanceFromJournal` | decimal | Баланс из журнала операций |
| `AccruedInterest` | decimal | Начисленные проценты |
| `ValueInRub` | decimal | Стоимость в рублях |

---

### InvestmentPositionModel

Инвестиционная позиция (акции, облигации, ETF).

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `InstrumentId` | int | FK на инструмент |
| `AccountId` | int | FK на брокерский счёт |
| `Quantity` | decimal | Количество |
| `AvgPurchasePrice` | decimal | Средняя цена покупки |
| `CurrentPrice` | decimal | Текущая цена |
| `IsAutoUpdateEnabled` | bool | Автообновление цены |
| `LastManualOverrideDate` | DateTime? | Дата ручного ввода |
| `UserUid` | string | Идентификатор пользователя |

**Вычисляемые поля (NotMapped):**
| Поле | Тип | Описание |
|------|-----|----------|
| `QuantityFromJournal` | decimal | Количество из журнала |
| `AvgPriceFromJournal` | decimal | Средняя цена из журнала |
| `DividendsFromJournal` | decimal | Полученные дивиденды |
| `ReturnPercent` | decimal | Доходность (%) |
| `ValueInRub` | decimal | Стоимость в рублях |

---

### CryptoAssetModel

Криптовалютный актив.

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `InstrumentId` | int | FK на инструмент |
| `AccountId` | int | FK на счёт (биржа/кошелёк) |
| `Ticker` | string | Тикер (BTC, ETH) |
| `CoinGeckoId` | string | ID для CoinGecko API |
| `CurrencyId` | string | Валюта учёта |
| `Quantity` | decimal(18,8) | Количество (8 знаков после запятой) |
| `AvgPurchasePrice` | decimal | Средняя цена покупки |
| `CurrentPrice` | decimal | Текущая цена |
| `IsAutoUpdateEnabled` | bool | Автообновление цены |
| `WalletAddress` | string | Адрес кошелька |
| `Notes` | string | Заметки |
| `UserUid` | string | Идентификатор пользователя |

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
| `Name` | string | Название (слиток, монета) |
| `Purity` | decimal | Проба (999, 585) |
| `WeightGrams` | decimal | Вес единицы (грамм) |
| `Quantity` | int | Количество единиц |
| `PurchasePrice` | decimal | Цена покупки за единицу |
| `PurchaseDate` | DateTime | Дата покупки |
| `CurrentPricePerGram` | decimal | Текущая цена за грамм |
| `IsAutoUpdateEnabled` | bool | Автообновление цены |
| `LastManualOverrideDate` | DateTime? | Дата ручного ввода |
| `UserUid` | string | Идентификатор пользователя |

**Вычисляемые поля (NotMapped):**
| Поле | Тип | Описание |
|------|-----|----------|
| `TotalWeightGrams` | decimal | Общий вес |
| `TotalValueRub` | decimal | Общая стоимость в рублях |

---

## Модели операций

### OperationModel

Журнал операций (транзакции).

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Date` | DateTime | Дата операции |
| `OperationTypeId` | int | FK на тип операции |
| `AccountId` | int | FK на счёт |
| `InstrumentId` | int? | FK на инструмент (опционально) |
| `Quantity` | decimal | Количество |
| `Price` | decimal | Цена за единицу |
| `CurrencyId` | string | Валюта операции |
| `ExchangeRateToRub` | decimal(18,6) | Курс к рублю на дату |
| `AmountInRub` | decimal(18,2) | Сумма в рублях |
| `Commission` | decimal? | Комиссия (RUB) |
| `Tax` | decimal? | Налог (RUB) |
| `Notes` | string(500) | Комментарий |
| `UserUid` | string | Идентификатор пользователя |

**Связи:**
- `OperationType` — тип операции
- `Account` — счёт
- `Instrument` — инструмент
- `Currency` — валюта

---

### OperationTypeModel

Справочник типов операций.

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Name` | string | Название типа |
| `Category` | OperationCategory | Категория |
| `UserUid` | string | Идентификатор пользователя |

**Предустановленные типы (15 шт.):**
- Пополнение вклада, Снятие со вклада, Начисление процентов
- Покупка ценных бумаг, Продажа ценных бумаг
- Получение дивидендов, Получение купонов
- Покупка криптовалюты, Продажа криптовалюты
- Покупка драгметалла, Продажа драгметалла
- Перевод между счетами
- Прочие расходы, Прочие доходы

---

## Модели истории

### PriceHistoryModel

История цен инструментов.

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `InstrumentId` | int | FK на инструмент |
| `Date` | DateTime | Дата |
| `Price` | decimal | Цена в валюте инструмента |
| `CurrencyId` | string | Валюта |
| `PriceInRub` | decimal | Цена в рублях |
| `Source` | PriceSource | Источник данных |

---

### InflationModel

Данные об инфляции (ИПЦ).

| Поле | Тип | Описание |
|------|-----|----------|
| `Id` | int | Первичный ключ |
| `Year` | int | Год |
| `Month` | int | Месяц (1-12) |
| `CpiPercent` | decimal(5,2) | ИПЦ за месяц (%) |
| `AccumulatedYearPercent` | decimal(8,4) | Накопленная инфляция за год (%) |
| `UserUid` | string | Идентификатор пользователя |

---

## Диаграмма связей

```
┌────────────────┐     ┌────────────────┐     ┌────────────────┐
│    Currency    │────<│ ExchangeRate   │     │   Inflation    │
│  (Code = PK)   │     │   (history)    │     │   (monthly)    │
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
