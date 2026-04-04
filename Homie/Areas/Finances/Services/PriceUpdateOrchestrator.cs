using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Homie.Areas.Finances.Models;
using Homie.Data.Models;

namespace Homie.Areas.Finances.Services
{
    /// <summary>Координатор обновления цен из всех источников</summary>
    public class PriceUpdateOrchestrator : IPriceUpdateOrchestrator
    {
        private readonly ApplicationDbContext _db;
        private readonly IMoexPriceService _moex;
        private readonly IYahooFinancePriceService _yahoo;
        private readonly ICoinGeckoPriceService _coinGecko;
        private readonly ICbrExchangeRateService _cbr;
        private readonly FinancesSettings _settings;
        private readonly ILogger<PriceUpdateOrchestrator> _logger;

        public PriceUpdateOrchestrator(
            ApplicationDbContext db,
            IMoexPriceService moex,
            IYahooFinancePriceService yahoo,
            ICoinGeckoPriceService coinGecko,
            ICbrExchangeRateService cbr,
            IOptions<FinancesSettings> settings,
            ILogger<PriceUpdateOrchestrator> logger)
        {
            _db = db;
            _moex = moex;
            _yahoo = yahoo;
            _coinGecko = coinGecko;
            _cbr = cbr;
            _settings = settings.Value;
            _logger = logger;
        }

        /// <summary>Возвращает ExternalCode если он задан и выглядит как валидный тикер, иначе Code</summary>
        private static string GetTickerKey(InstrumentModel i) =>
            !string.IsNullOrWhiteSpace(i.ExternalCode) && !i.ExternalCode.Contains(' ')
                ? i.ExternalCode
                : i.Code;

        public async Task<PriceUpdateResultViewModel> UpdateAllPricesAsync(string userId)
        {
            var result = new PriceUpdateResultViewModel { UpdatedAt = DateTime.UtcNow };
            var now = DateTime.UtcNow;
            var lockHours = _settings.ManualOverrideLockHours;

            // Загружаем все инструменты пользователя
            var instruments = await _db.Instruments
                .Where(i => i.UserUid == userId)
                .ToListAsync();

            // Загружаем позиции для проверки auto-update флагов
            var investments = await _db.InvestmentPositions
                .Where(ip => ip.UserUid == userId && ip.IsAutoUpdateEnabled)
                .Include(ip => ip.Instrument)
                .ToListAsync();

            var cryptoAssets = await _db.CryptoAssets
                .Where(ca => ca.UserUid == userId && ca.IsAutoUpdateEnabled)
                .Include(ca => ca.Instrument)
                .ToListAsync();

            var metals = await _db.PreciousMetals
                .Where(pm => pm.UserUid == userId && pm.IsAutoUpdateEnabled)
                .ToListAsync();

            // --- 1. MOEX (раздельно: акции, облигации, ETF) ---
            var moexInstruments = instruments.Where(i => i.Exchange == Exchange.MOEX).ToList();

            var moexStockTickers = moexInstruments
                .Where(i => i.Type == InstrumentType.Stock)
                .Select(i => GetTickerKey(i))
                .Distinct().ToList();

            var moexBondTickers = moexInstruments
                .Where(i => i.Type == InstrumentType.Bond)
                .Select(i => GetTickerKey(i))
                .Distinct().ToList();

            var moexEtfTickers = moexInstruments
                .Where(i => i.Type == InstrumentType.ETF)
                .Select(i => GetTickerKey(i))
                .Distinct().ToList();

            var moexStockPrices = moexStockTickers.Any()
                ? await _moex.FetchSharePricesAsync(moexStockTickers)
                : new Dictionary<string, decimal>();

            var moexBondPrices = moexBondTickers.Any()
                ? await _moex.FetchBondPricesAsync(moexBondTickers)
                : new Dictionary<string, decimal>();

            var moexEtfPrices = moexEtfTickers.Any()
                ? await _moex.FetchEtfPricesAsync(moexEtfTickers)
                : new Dictionary<string, decimal>();

            // Объединяем все MOEX-цены
            var moexPrices = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in moexStockPrices) moexPrices[kv.Key] = kv.Value;
            foreach (var kv in moexBondPrices) moexPrices[kv.Key] = kv.Value;
            foreach (var kv in moexEtfPrices) moexPrices[kv.Key] = kv.Value;

            // --- 2. Yahoo Finance ---
            var yahooExchanges = new[] { Exchange.NYSE, Exchange.NASDAQ, Exchange.LSE };
            var yahooTickers = instruments
                .Where(i => yahooExchanges.Contains(i.Exchange))
                .Select(i => GetTickerKey(i))
                .Distinct().ToList();

            var yahooPrices = yahooTickers.Any()
                ? await _yahoo.FetchPricesAsync(yahooTickers)
                : new Dictionary<string, decimal>();

            // --- 3. CoinGecko ---
            var cryptoIds = instruments
                .Where(i => i.Exchange == Exchange.CryptoSpot && !string.IsNullOrEmpty(i.ExternalCode))
                .Select(i => i.ExternalCode)
                .Distinct().ToList();

            var cryptoPrices = cryptoIds.Any()
                ? await _coinGecko.FetchPricesAsync(cryptoIds)
                : new Dictionary<string, CryptoPriceDto>();

            // --- 4. ЦБ металлы ---
            var metalPrices = metals.Any()
                ? await _cbr.FetchMetalPricesAsync(DateTime.Today)
                : new Dictionary<string, decimal>();

            // --- Обработка результатов ---

            // Обновляем инструменты (MOEX + Yahoo)
            foreach (var instrument in instruments)
            {
                var itemResult = new PriceUpdateItemResult
                {
                    InstrumentName = instrument.Name,
                    Ticker = instrument.Code,
                    OldPrice = instrument.LastPrice
                };

                var tickerKey = GetTickerKey(instrument);
                decimal? newPrice = null;
                PriceSource source = PriceSource.Manual;

                if (instrument.Exchange == Exchange.MOEX && moexPrices.TryGetValue(tickerKey, out var mp))
                {
                    newPrice = mp;
                    source = PriceSource.MoexAuto;
                }
                else if (yahooExchanges.Contains(instrument.Exchange) && yahooPrices.TryGetValue(tickerKey, out var yp))
                {
                    newPrice = yp;
                    source = PriceSource.YahooAuto;
                }
                else if (instrument.Exchange == Exchange.CryptoSpot && 
                         !string.IsNullOrEmpty(instrument.ExternalCode) &&
                         cryptoPrices.TryGetValue(instrument.ExternalCode, out var cp))
                {
                    newPrice = cp.Usd; // Храним в валюте инструмента (обычно USD для крипто)
                    source = PriceSource.CoinGeckoAuto;
                }

                if (newPrice.HasValue)
                {
                    instrument.LastPrice = newPrice.Value;
                    instrument.LastPriceDate = now;
                    instrument.LastPriceSource = source;

                    // Записываем в PriceHistory
                    var existing = await _db.PriceHistory
                        .FirstOrDefaultAsync(ph => ph.InstrumentId == instrument.Id 
                                                    && ph.Date.Date == DateTime.Today 
                                                    && ph.UserUid == userId);
                    if (existing != null)
                    {
                        existing.Price = newPrice.Value;
                        existing.Source = source;
                    }
                    else
                    {
                        _db.PriceHistory.Add(new PriceHistoryModel
                        {
                            InstrumentId = instrument.Id,
                            Date = DateTime.Today,
                            Price = newPrice.Value,
                            CurrencyId = instrument.CurrencyId,
                            PriceInRub = 0, // Будет пересчитано позже
                            Source = source,
                            UserUid = userId
                        });
                    }

                    itemResult.NewPrice = newPrice.Value;
                    itemResult.Source = source;
                    itemResult.Success = true;
                    result.SuccessCount++;
                }
                else
                {
                    itemResult.Success = false;
                    itemResult.ErrorMessage = "Цена не найдена";
                    result.ErrorCount++;
                }

                result.Items.Add(itemResult);
                result.TotalProcessed++;
            }

            // Обновляем текущие цены на инвест-позициях
            foreach (var pos in investments)
            {
                if (pos.LastManualOverrideDate.HasValue &&
                    (now - pos.LastManualOverrideDate.Value).TotalHours < lockHours)
                    continue; // Ручная корректировка ещё действует

                if (pos.Instrument?.LastPrice.HasValue == true)
                {
                    pos.CurrentPrice = pos.Instrument.LastPrice.Value;
                }
            }

            // Обновляем текущие цены на криптоактивах
            foreach (var ca in cryptoAssets)
            {
                if (ca.LastManualOverrideDate.HasValue &&
                    (now - ca.LastManualOverrideDate.Value).TotalHours < lockHours)
                    continue;

                if (ca.Instrument?.ExternalCode != null &&
                    cryptoPrices.TryGetValue(ca.Instrument.ExternalCode, out var cpDto))
                {
                    ca.CurrentPrice = cpDto.Usd;
                }
            }

            // Обновляем цены металлов
            foreach (var metal in metals)
            {
                if (metal.LastManualOverrideDate.HasValue &&
                    (now - metal.LastManualOverrideDate.Value).TotalHours < lockHours)
                    continue;

                var metalName = metal.Metal.ToString();
                if (metalPrices.TryGetValue(metalName, out var metalPrice))
                {
                    metal.CurrentPricePerGram = metalPrice;
                }
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("Обновление цен завершено: {Success} успешно, {Errors} ошибок, {Skipped} пропущено",
                result.SuccessCount, result.ErrorCount, result.SkippedCount);

            return result;
        }

        public async Task<PriceUpdateItemResult> UpdateSinglePriceAsync(int instrumentId, string userId)
        {
            var instrument = await _db.Instruments
                .FirstOrDefaultAsync(i => i.Id == instrumentId && i.UserUid == userId);

            if (instrument == null)
                return new PriceUpdateItemResult { Success = false, ErrorMessage = "Инструмент не найден" };

            var itemResult = new PriceUpdateItemResult
            {
                InstrumentName = instrument.Name,
                Ticker = instrument.Code,
                OldPrice = instrument.LastPrice
            };

            var tickerKey = GetTickerKey(instrument);
            decimal? newPrice = null;
            PriceSource source = PriceSource.Manual;

            try
            {
                switch (instrument.Exchange)
                {
                    case Exchange.MOEX:
                        Dictionary<string, decimal> moexResult;
                        if (instrument.Type == InstrumentType.Bond)
                            moexResult = await _moex.FetchBondPricesAsync(new[] { tickerKey });
                        else if (instrument.Type == InstrumentType.ETF)
                            moexResult = await _moex.FetchEtfPricesAsync(new[] { tickerKey });
                        else
                            moexResult = await _moex.FetchSharePricesAsync(new[] { tickerKey });

                        if (moexResult.TryGetValue(tickerKey, out var mp))
                        {
                            newPrice = mp;
                            source = PriceSource.MoexAuto;
                        }
                        break;

                    case Exchange.NYSE:
                    case Exchange.NASDAQ:
                    case Exchange.LSE:
                        var yahooResult = await _yahoo.FetchPricesAsync(new[] { tickerKey });
                        if (yahooResult.TryGetValue(tickerKey, out var yp))
                        {
                            newPrice = yp;
                            source = PriceSource.YahooAuto;
                        }
                        break;

                    case Exchange.CryptoSpot:
                        if (!string.IsNullOrEmpty(instrument.ExternalCode))
                        {
                            var cgResult = await _coinGecko.FetchPricesAsync(new[] { instrument.ExternalCode });
                            if (cgResult.TryGetValue(instrument.ExternalCode, out var cp))
                            {
                                newPrice = cp.Usd;
                                source = PriceSource.CoinGeckoAuto;
                            }
                        }
                        break;
                }

                if (newPrice.HasValue)
                {
                    instrument.LastPrice = newPrice.Value;
                    instrument.LastPriceDate = DateTime.UtcNow;
                    instrument.LastPriceSource = source;

                    var existing = await _db.PriceHistory
                        .FirstOrDefaultAsync(ph => ph.InstrumentId == instrument.Id
                                                    && ph.Date.Date == DateTime.Today
                                                    && ph.UserUid == userId);
                    if (existing != null)
                    {
                        existing.Price = newPrice.Value;
                        existing.Source = source;
                    }
                    else
                    {
                        _db.PriceHistory.Add(new PriceHistoryModel
                        {
                            InstrumentId = instrument.Id,
                            Date = DateTime.Today,
                            Price = newPrice.Value,
                            CurrencyId = instrument.CurrencyId,
                            PriceInRub = 0,
                            Source = source,
                            UserUid = userId
                        });
                    }

                    await _db.SaveChangesAsync();

                    // Обновляем текущую цену на инвест-позициях этого инструмента
                    var positions = await _db.InvestmentPositions
                        .Where(ip => ip.InstrumentId == instrument.Id)
                        .ToListAsync();
                    foreach (var pos in positions)
                    {
                        pos.CurrentPrice = newPrice.Value;
                    }
                    if (positions.Any())
                        await _db.SaveChangesAsync();

                    itemResult.NewPrice = newPrice.Value;
                    itemResult.Source = source;
                    itemResult.Success = true;
                }
                else
                {
                    itemResult.Success = false;
                    itemResult.ErrorMessage = "Цена не найдена на источнике";
                }
            }
            catch (Exception ex)
            {
                itemResult.Success = false;
                itemResult.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Ошибка обновления цены для {Ticker}", instrument.Code);
            }

            return itemResult;
        }
    }
}
