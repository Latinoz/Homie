# Интеграции с внешними API

## Обзор

Модуль Finances интегрирован с 4 внешними источниками данных для автоматического получения котировок и курсов валют.

| Сервис | API | Данные |
|--------|-----|--------|
| ICbrExchangeRateService | ЦБ РФ | Курсы валют, цены драгметаллов |
| IMoexPriceService | MOEX ISS | Акции и облигации на Московской бирже |
| IYahooFinancePriceService | Yahoo Finance | Международные акции и ETF |
| ICoinGeckoPriceService | CoinGecko | Криптовалюты |

---

## ICbrExchangeRateService — Центральный банк России

### Назначение

Получение официальных курсов валют и учётных цен драгоценных металлов от ЦБ РФ.

### Методы интерфейса

```csharp
public interface ICbrExchangeRateService
{
    /// <summary>
    /// Получить курсы валют на указанную дату
    /// </summary>
    Task<Dictionary<string, decimal>> FetchCurrencyRatesAsync(DateTime date);
    
    /// <summary>
    /// Получить цены драгоценных металлов на указанную дату
    /// </summary>
    Task<Dictionary<MetalType, decimal>> FetchMetalPricesAsync(DateTime date);
}
```

### API Endpoints

| Endpoint | Описание |
|----------|----------|
| `https://www.cbr.ru/scripts/XML_daily.asp?date_req=DD/MM/YYYY` | Курсы валют |
| `https://www.cbr.ru/scripts/xml_metall.asp?date_req1=DD/MM/YYYY&date_req2=DD/MM/YYYY` | Цены металлов |

### Формат ответа (курсы валют)

```xml
<?xml version="1.0" encoding="windows-1251"?>
<ValCurs Date="09.03.2026" name="Foreign Currency Market">
    <Valute ID="R01235">
        <NumCode>840</NumCode>
        <CharCode>USD</CharCode>
        <Nominal>1</Nominal>
        <Name>Доллар США</Name>
        <Value>92,5000</Value>
        <VunitRate>92,5000</VunitRate>
    </Valute>
    <Valute ID="R01239">
        <NumCode>978</NumCode>
        <CharCode>EUR</CharCode>
        <Nominal>1</Nominal>
        <Name>Евро</Name>
        <Value>100,1234</Value>
        <VunitRate>100,1234</VunitRate>
    </Valute>
</ValCurs>
```

### Формат ответа (драгметаллы)

```xml
<?xml version="1.0" encoding="windows-1251"?>
<Metall>
    <Record Date="09.03.2026" Code="1">
        <Buy>5500.00</Buy>
        <Sell>5550.00</Sell>
    </Record>
    <!-- Code: 1=Gold, 2=Silver, 3=Platinum, 4=Palladium -->
</Metall>
```

### Расписание обновления

- **Время:** 08:30 UTC (настраивается в `CbrUpdateTimeUtc`)
- **Частота:** Ежедневно по рабочим дням

---

## IMoexPriceService — Московская биржа

### Назначение

Получение котировок акций и облигаций, торгуемых на Московской бирже.

### Методы интерфейса

```csharp
public interface IMoexPriceService
{
    /// <summary>
    /// Получить цены акций по списку тикеров
    /// </summary>
    Task<Dictionary<string, decimal>> FetchSharePricesAsync(IEnumerable<string> tickers);
    
    /// <summary>
    /// Получить цены облигаций (% от номинала)
    /// </summary>
    Task<Dictionary<string, decimal>> FetchBondPricesAsync(IEnumerable<string> tickers);
}
```

### API Endpoints (MOEX ISS)

| Endpoint | Описание |
|----------|----------|
| `https://iss.moex.com/iss/engines/stock/markets/shares/securities/{ticker}.json` | Акции |
| `https://iss.moex.com/iss/engines/stock/markets/bonds/securities/{ticker}.json` | Облигации |
| `https://iss.moex.com/iss/engines/stock/markets/shares/boards/TQBR/securities.json` | Все акции |

### Формат ответа (JSON)

```json
{
  "marketdata": {
    "columns": ["SECID", "LAST", "OPEN", "HIGH", "LOW", "VOLTODAY"],
    "data": [
      ["SBER", 285.50, 284.00, 286.00, 283.50, 15000000]
    ]
  }
}
```

### Поддерживаемые инструменты

- **Акции:** SBER, GAZP, LKOH, GMKN, NVTK, ROSN, TATN, MGNT...
- **Облигации:** ОФЗ, корпоративные облигации
- **ETF:** FXGD, FXUS, FXCN...

### Режимы торгов

| Board | Описание |
|-------|----------|
| TQBR | Т+ акции |
| TQCB | Т+ облигации |
| TQTF | Т+ ETF |

---

## IYahooFinancePriceService — Yahoo Finance

### Назначение

Получение котировок международных акций и ETF с бирж NYSE, NASDAQ, LSE.

### Методы интерфейса

```csharp
public interface IYahooFinancePriceService
{
    /// <summary>
    /// Получить цены по списку тикеров
    /// </summary>
    Task<Dictionary<string, decimal>> FetchPricesAsync(IEnumerable<string> tickers);
}
```

### API Endpoint

```
https://query1.finance.yahoo.com/v8/finance/chart/{ticker}?interval=1d&range=1d
```

### Формат ответа (JSON)

```json
{
  "chart": {
    "result": [{
      "meta": {
        "symbol": "AAPL",
        "regularMarketPrice": 175.50,
        "currency": "USD"
      },
      "indicators": {
        "quote": [{
          "close": [175.50],
          "open": [174.00],
          "high": [176.00],
          "low": [173.50]
        }]
      }
    }]
  }
}
```

### Формат тикеров

| Биржа | Формат | Пример |
|-------|--------|--------|
| NYSE | `TICKER` | AAPL, MSFT |
| NASDAQ | `TICKER` | GOOGL, AMZN |
| LSE | `TICKER.L` | HSBA.L, BP.L |
| Frankfurt | `TICKER.DE` | SAP.DE |

### Rate Limiting

- **Задержка:** 1000 мс между запросами (`RequestDelayMs`)
- **Лимит:** ~2000 запросов/час (без API-ключа)

---

## ICoinGeckoPriceService — CoinGecko

### Назначение

Получение котировок криптовалют в USD и RUB.

### Методы интерфейса

```csharp
public interface ICoinGeckoPriceService
{
    /// <summary>
    /// Получить цены криптовалют по CoinGecko ID
    /// </summary>
    Task<Dictionary<string, CryptoPriceDto>> FetchPricesAsync(IEnumerable<string> coinGeckoIds);
}

public class CryptoPriceDto
{
    public decimal PriceUsd { get; set; }
    public decimal PriceRub { get; set; }
    public decimal Change24h { get; set; }
    public decimal MarketCap { get; set; }
}
```

### API Endpoints

| Endpoint | Описание |
|----------|----------|
| `https://api.coingecko.com/api/v3/simple/price` | Текущие цены |
| `https://api.coingecko.com/api/v3/coins/{id}` | Детали монеты |
| `https://api.coingecko.com/api/v3/coins/{id}/market_chart` | История цен |

### Пример запроса

```
GET https://api.coingecko.com/api/v3/simple/price
    ?ids=bitcoin,ethereum,tether
    &vs_currencies=usd,rub
    &include_24hr_change=true
    &include_market_cap=true
```

### Формат ответа

```json
{
  "bitcoin": {
    "usd": 65000.00,
    "rub": 6012500.00,
    "usd_24h_change": 2.5,
    "usd_market_cap": 1280000000000
  },
  "ethereum": {
    "usd": 3500.00,
    "rub": 323750.00,
    "usd_24h_change": 1.8,
    "usd_market_cap": 420000000000
  }
}
```

### CoinGecko ID

| Тикер | CoinGecko ID |
|-------|--------------|
| BTC | bitcoin |
| ETH | ethereum |
| USDT | tether |
| BNB | binancecoin |
| SOL | solana |
| XRP | ripple |
| ADA | cardano |

### Rate Limiting

- **Задержка:** 2100 мс между запросами (`RequestDelayMs`)
- **Лимит:** 10-50 запросов/минуту (бесплатный план)

---

## IPriceUpdateOrchestrator — Оркестратор

### Назначение

Координация всех источников данных и массовое обновление котировок.

### Методы интерфейса

```csharp
public interface IPriceUpdateOrchestrator
{
    /// <summary>
    /// Обновить все котировки для пользователя
    /// </summary>
    Task<PriceUpdateResultViewModel> UpdateAllPricesAsync(string userId);
    
    /// <summary>
    /// Обновить цену одного инструмента
    /// </summary>
    Task<PriceUpdateItemResult> UpdateSinglePriceAsync(int instrumentId, string userId);
}
```

### Алгоритм обновления

```
1. Получить список активных инструментов пользователя
2. Сгруппировать по источнику данных:
   - MOEX: акции/облигации с Exchange = MOEX
   - Yahoo: инструменты с Exchange = NYSE/NASDAQ/LSE
   - CoinGecko: инструменты с Type = Crypto
   - CBR: драгметаллы с Type = PreciousMetalRef
3. Для каждой группы:
   a. Выполнить запрос к API
   b. Обновить LastPrice, LastPriceDate, LastPriceSource
   c. Сохранить в PriceHistory
   d. Записать результат (успех/ошибка)
4. Обновить курсы валют через CBR
5. Вернуть сводку результатов
```

### Результат обновления

```csharp
public class PriceUpdateResultViewModel
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public int SkippedCount { get; set; }  // IsAutoUpdateEnabled = false
    public DateTime UpdateTime { get; set; }
    public List<PriceUpdateItemResult> Items { get; set; }
}

public class PriceUpdateItemResult
{
    public int InstrumentId { get; set; }
    public string InstrumentCode { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal? NewPrice { get; set; }
    public PriceSource Source { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}
```

---

## PriceUpdateHostedService — Фоновый сервис

### Назначение

Автоматическое обновление котировок по расписанию.

### Конфигурация

```json
{
  "Finances": {
    "EnableBackgroundPriceUpdate": true,
    "CbrUpdateTimeUtc": "08:30",
    "PriceUpdateTimeUtc": "19:00"
  }
}
```

### Логика работы

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

            var now = DateTime.UtcNow;
            
            // Обновление курсов ЦБ в 08:30 UTC
            if (ShouldUpdateCbr(now))
            {
                await UpdateCbrRatesAsync();
            }
            
            // Обновление котировок в 19:00 UTC (после закрытия MOEX)
            if (ShouldUpdatePrices(now))
            {
                await UpdateAllPricesAsync();
            }
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

### Расписание

| Событие | Время (UTC) | Причина |
|---------|-------------|---------|
| Курсы ЦБ | 08:30 | После публикации ЦБ (~08:00 UTC) |
| Котировки | 19:00 | После закрытия MOEX (15:50 UTC) и перед открытием US (13:30 UTC) |

---

## Блокировка ручного ввода

### ManualOverrideLockHours

После ручного ввода цены автообновление блокируется на N часов:

```csharp
// При ручном вводе
position.CurrentPrice = manualPrice;
position.LastManualOverrideDate = DateTime.UtcNow;

// При автообновлении
if (position.LastManualOverrideDate.HasValue)
{
    var lockExpires = position.LastManualOverrideDate.Value
        .AddHours(_settings.ManualOverrideLockHours);
    
    if (DateTime.UtcNow < lockExpires)
    {
        // Пропускаем автообновление
        return;
    }
}
```

По умолчанию: 24 часа.

---

## Обработка ошибок

### Типичные ошибки

| Код | Описание | Действие |
|-----|----------|----------|
| 429 | Too Many Requests | Увеличить задержку между запросами |
| 404 | Инструмент не найден | Проверить тикер/CoinGecko ID |
| 5xx | Сервер недоступен | Повторить позже |
| Timeout | Превышено время ожидания | Увеличить timeout |

### Retry-политика

```csharp
// Используется Polly для повторных попыток
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<TaskCanceledException>()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
```
