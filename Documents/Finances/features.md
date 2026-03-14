# Функциональность модуля Finances

## Обзор контроллеров

Модуль содержит 16 контроллеров, обеспечивающих полный CRUD и специализированные операции.

| Контроллер | Назначение | Маршрут |
|------------|-----------|---------|
| DashboardController | Сводная информация | /Finances/Dashboard |
| AccountsController | Управление счетами | /Finances/Accounts |
| BanksController | Справочник банков | /Finances/Banks |
| BrokersController | Справочник брокеров | /Finances/Brokers |
| WalletsController | Справочник кошельков | /Finances/Wallets |
| CryptoExchangesController | Справочник криптобирж | /Finances/CryptoExchanges |
| CurrenciesController | Управление валютами | /Finances/Currencies |
| DepositsController | Банковские вклады | /Finances/Deposits |
| InvestmentsController | Инвестиционные позиции | /Finances/Investments |
| CryptoController | Криптовалютные активы | /Finances/Crypto |
| PreciousMetalsController | Драгоценные металлы | /Finances/PreciousMetals |
| OperationsController | Журнал операций | /Finances/Operations |
| OperationTypesController | Типы операций | /Finances/OperationTypes |
| InstrumentsController | Финансовые инструменты | /Finances/Instruments |
| PriceUpdateController | Обновление котировок | /Finances/PriceUpdate |
| InflationController | Данные инфляции | /Finances/Inflation |

---

## DashboardController — Дашборд

Главная страница модуля с обзором портфеля.

### Функции

| Action | HTTP | Описание |
|--------|------|----------|
| `Index` | GET | Отображение сводной информации |
| `UpdateAllPrices` | POST | Ручное обновление всех котировок |

### Отображаемые данные

**Сводка по активам:**
- `TotalCapital` — общий капитал в рублях
- `TotalDeposits` — сумма вкладов
- `TotalInvestments` — стоимость инвестиций
- `TotalCrypto` — стоимость криптовалюты
- `TotalPreciousMetals` — стоимость драгметаллов

**Аналитика:**
- `TotalReturnPercent` — общая доходность портфеля
- `RealValueAfterInflation` — реальная стоимость с учётом инфляции
- `AccumulatedInflationPercent` — накопленная инфляция

**Графики (Chart.js):**
- Круговая диаграмма распределения активов (`AssetAllocationJson`)
- Линейный график динамики капитала (`CapitalDynamicsJson`)

**Виджеты:**
- Последние курсы валют (`LatestRates`)
- Недавние операции (`RecentOperations`)
- Дата последнего обновления цен (`LastPriceUpdateDate`)

---

## AccountsController — Счета

Управление финансовыми счетами.

### Функции

| Action | HTTP | Описание |
|--------|------|----------|
| `Index` | GET | Список счетов с фильтрацией |
| `Create` | GET/POST | Создание нового счёта |
| `Edit` | GET/POST | Редактирование счёта |
| `Delete` | GET/POST | Удаление счёта |

### Особенности

- Фильтрация по типу счёта (Bank/Broker/Wallet/CryptoExchange)
- Фильтрация по названию
- Сортировка по имени, типу
- Пагинация

### Связанные сущности

При создании счёта можно выбрать:
- Банк (для типа Bank)
- Брокера (для типа Broker)
- Кошелёк (для типа Wallet)
- Криптобиржу (для типа CryptoExchange)

---

## DepositsController — Банковские вклады

### Функции

| Action | HTTP | Описание |
|--------|------|----------|
| `Index` | GET | Список вкладов |
| `Details` | GET | Детали вклада с расчётом баланса |
| `Create` | GET/POST | Открытие нового вклада |
| `Edit` | GET/POST | Редактирование параметров |
| `Delete` | GET/POST | Закрытие/удаление вклада |

### Расчёт баланса

Баланс вклада рассчитывается из журнала операций:

```
Баланс = Σ(Пополнения) - Σ(Снятия) + Σ(Начисленные проценты)
```

### Поля формы

- Название вклада
- Счёт (выбор из списка банковских счетов)
- Валюта
- Начальная сумма
- Процентная ставка (%)
- Дата открытия / закрытия
- Капитализация процентов (да/нет)
- Условия (текстовое поле)

---

## InvestmentsController — Инвестиции

### Функции

| Action | HTTP | Описание |
|--------|------|----------|
| `Index` | GET | Список позиций |
| `Details` | GET | Детали позиции с метриками и графиком цен |
| `Create` | GET/POST | Добавление позиции |
| `Edit` | GET/POST | Редактирование (поддержка `manualPriceOverride`) |
| `Delete` | GET/POST | Удаление позиции |
| `UpdatePrice` | POST | Обновление цены через API |

### Расчёт метрик

```
Количество = Σ(Покупки) - Σ(Продажи)
Средняя цена = Σ(Цена × Количество) / Σ(Количество)
Доходность (%) = (Текущая цена - Средняя цена) / Средняя цена × 100
Стоимость (RUB) = Количество × Текущая цена × Курс валюты
```

### Поля формы

- Инструмент (выбор из справочника)
- Брокерский счёт
- Количество
- Средняя цена покупки
- Автообновление цены (да/нет)

---

## CryptoController — Криптовалюта

### Функции

| Action | HTTP | Описание |
|--------|------|----------|
| `Index` | GET | Список криптоактивов |
| `Details` | GET | Детали актива с графиком цен |
| `Create` | GET/POST | Добавление актива |
| `Edit` | GET/POST | Редактирование (поддержка `manualPriceOverride`) |
| `Delete` | GET/POST | Удаление |
| `UpdatePrice` | POST | Обновление цены через CoinGecko |

### Особенности

- Поддержка 8 знаков после запятой для количества
- Интеграция с CoinGecko API
- Адрес кошелька для отслеживания

### Поля формы

- Тикер (BTC, ETH и др.)
- CoinGecko ID (для API)
- Счёт (биржа или кошелёк)
- Количество
- Валюта учёта
- Средняя цена покупки
- Адрес кошелька (опционально)

---

## PreciousMetalsController — Драгоценные металлы

### Функции

| Action | HTTP | Описание |
|--------|------|----------|
| `Index` | GET | Список металлов с итоговой стоимостью (₽) по типам |
| `Create` | GET/POST | Добавление металла |
| `Edit` | GET/POST | Редактирование (поддержка `manualPriceOverride`) |
| `Delete` | GET/POST | Удаление |

### Итоги по типам металлов

На странице списка отображаются итоговые стоимости в рублях:
- TotalGoldRub — стоимость всего золота (₽)
- TotalSilverRub — стоимость всего серебра (₽)
- TotalPlatinumRub — стоимость всей платины (₽)
- TotalPalladiumRub — стоимость всего палладия (₽)

### Поля формы

- Тип металла (Gold/Silver/Platinum/Palladium)
- Название (слиток 50г, монета «Георгий Победоносец»)
- Проба (текстовое поле: «999», «585»)
- Вес единицы (грамм)
- Количество единиц
- Цена покупки за единицу
- Дата покупки (опционально)
- Автообновление цены (да/нет)

---

## OperationsController — Журнал операций

### Функции

| Action | HTTP | Описание |
|--------|------|----------|
| `Index` | GET | Список операций с фильтрами |
| `Create` | GET/POST | Добавление операции |
| `Edit` | GET/POST | Редактирование |
| `Delete` | GET/POST | Удаление |

### Фильтры

- По категории (Deposit/Investment/Crypto/PreciousMetal/Transfer/Other)
- По счёту
- По дате (от — до)
- Текстовый поиск

### Поля формы

- Дата операции
- Тип операции (выбор из справочника)
- Счёт
- Инструмент (опционально)
- Количество
- Цена
- Валюта
- Курс к рублю (автозаполнение)
- Сумма в рублях (автозаполнение)
- Комиссия
- Налог
- Комментарий

---

## InstrumentsController — Инструменты

### Функции

| Action | HTTP | Описание |
|--------|------|----------|
| `Index` | GET | Справочник инструментов |
| `Create` | GET/POST | Добавление инструмента |
| `Edit` | GET/POST | Редактирование |
| `Delete` | GET/POST | Удаление |

### Поля формы

- Код (тикер)
- Название
- Тип (Stock/Bond/ETF/Crypto/PreciousMetalRef)
- Биржа (MOEX/NYSE/NASDAQ/LSE/CryptoSpot)
- ISIN (для акций/облигаций)
- Внешний код (для API)
- Валюта
- Категория

---

## CurrenciesController — Валюты

### Функции

| Action | HTTP | Описание |
|--------|------|----------|
| `Index` | GET | Список валют с текущими курсами |
| `Create` | GET/POST | Добавление валюты |
| `Edit` | GET/POST | Редактирование |
| `Delete` | GET/POST | Удаление |
| `UpdateRatesFromCbr` | POST | Обновление курсов ЦБ РФ |
| `AddManualRate` | POST | Ручной ввод курса (currencyId, date, rate) |

### Предустановленные валюты

- RUB — Российский рубль (базовая)
- USD — Доллар США
- EUR — Евро
- CNY — Китайский юань
- GBP — Британский фунт

---

## PriceUpdateController — Обновление цен

### Функции

| Action | HTTP | Описание |
|--------|------|----------|
| `Index` | GET | Статус обновлений |
| `UpdateAll` | POST | Массовое обновление всех инструментов |
| `UpdateSingle` | POST | Обновление одного инструмента |
| `ManualEntry` | GET/POST | Ручной ввод цены |

### Ручной ввод цены (ManualEntry)

При `UpdateCurrentPrice = true` каскадно обновляет:
- `Instrument.LastPrice` / `LastPriceDate` / `LastPriceSource`
- `CurrentPrice` у всех связанных InvestmentPositions и CryptoAssets
- Устанавливает `LastManualOverrideDate` для блокировки автообновления

### Результат обновления

```csharp
public class PriceUpdateResultViewModel
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public int SkippedCount { get; set; }
    public List<PriceUpdateItemResult> Items { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

---

## InflationController — Инфляция

### Функции

| Action | HTTP | Описание |
|--------|------|----------|
| `Index` | GET | Данные инфляции по годам с фильтром |
| `Create` | GET/POST | Добавление месячных данных (автовычисление AccumulatedYearPercent) |
| `Edit` | GET/POST | Корректировка |
| `Delete` | GET/POST | Удаление |

### Автовычисление

При создании записи контроллер автоматически рассчитывает `AccumulatedYearPercent` на основе данных предыдущих месяцев текущего года. Форма создания предзаполняется текущим годом/месяцем.

### Формула накопленной инфляции

```
Накопленная = (1 + ИПЦ_янв) × (1 + ИПЦ_фев) × ... × (1 + ИПЦ_текущий) - 1
```

---

## Справочные контроллеры

### BanksController, BrokersController, WalletsController, CryptoExchangesController

Простые CRUD-контроллеры для справочников организаций.

| Action | HTTP | Описание |
|--------|------|----------|
| `Index` | GET | Список |
| `Create` | GET/POST | Создание |
| `Edit` | GET/POST | Редактирование |
| `Delete` | GET/POST | Удаление |

### OperationTypesController

Управление типами операций с привязкой к категориям.

**Защита от удаления:** при попытке удалить тип, который используется в операциях, контроллер проверяет наличие связанных записей и отклоняет удаление с сообщением об ошибке через TempData.

---

## Общие функции

### Пагинация

Все списки поддерживают пагинацию через `PageViewModel`:

```csharp
public class PageViewModel
{
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

### Сортировка

Списки поддерживают сортировку через `FinanceSortState`:

- По названию (↑↓)
- По дате (↑↓)
- По сумме (↑↓)
- По валюте (↑↓)
- По типу (↑↓)

### Авторизация

Все контроллеры защищены атрибутом:

```csharp
[Area("Finances")]
[Authorize(Roles = "admin,user,finances")]
public class DashboardController : Controller
```
