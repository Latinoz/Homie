using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Homie.Areas.Finances.Models;

namespace Homie.Areas.Finances.Services
{
    public class MoexPriceService : IMoexPriceService
    {
        private readonly HttpClient _httpClient;
        private readonly MoexIssSettings _settings;
        private readonly ILogger<MoexPriceService> _logger;

        public MoexPriceService(HttpClient httpClient, IOptions<FinancesSettings> options,
            ILogger<MoexPriceService> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value.MoexIss;
            _logger = logger;
        }

        public async Task<Dictionary<string, decimal>> FetchSharePricesAsync(IEnumerable<string> tickers)
        {
            return await FetchPricesFromBoard("stock", "shares", "TQBR", tickers);
        }

        public async Task<Dictionary<string, decimal>> FetchBondPricesAsync(IEnumerable<string> tickers)
        {
            // Облигации: ищем по бордам TQOB (ОФЗ) и TQCB (корп.), конвертируем % → абсолютная цена
            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var remaining = tickers.ToList();

            // Сначала TQOB (гос. облигации — ОФЗ)
            var fromTqob = await FetchBondPricesFromBoard("TQOB", remaining);
            foreach (var kv in fromTqob) result[kv.Key] = kv.Value;

            remaining = remaining.Where(t => !result.ContainsKey(t)).ToList();

            // Затем TQCB (корпоративные облигации)
            if (remaining.Any())
            {
                var fromTqcb = await FetchBondPricesFromBoard("TQCB", remaining);
                foreach (var kv in fromTqcb) result[kv.Key] = kv.Value;
            }

            _logger.LogInformation("MOEX Bonds: загружено {Count} цен", result.Count);
            return result;
        }

        public async Task<Dictionary<string, decimal>> FetchEtfPricesAsync(IEnumerable<string> tickers)
        {
            return await FetchPricesFromBoard("stock", "shares", "TQTF", tickers);
        }

        /// <summary>Безопасное чтение числового элемента строки данных MOEX (столбец может отсутствовать, напр. в выходные)</summary>
        private static bool TryGetNumber(JsonElement row, int index, out decimal value)
        {
            value = 0m;
            return row.GetArrayLength() > index
                && row[index].ValueKind == JsonValueKind.Number
                && row[index].TryGetDecimal(out value);
        }

        /// <summary>Загрузка цен облигаций с конвертацией из % номинала в абсолютную цену</summary>
        private async Task<Dictionary<string, decimal>> FetchBondPricesFromBoard(
            string board, IEnumerable<string> tickers)
        {
            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (var ticker in tickers)
            {
                try
                {
                    var url = $"{_settings.BaseUrl}/engines/stock/markets/bonds/boards/{board}/securities/{ticker}.json" +
                              "?iss.meta=off&iss.only=marketdata,securities" +
                              "&marketdata.columns=SECID,LAST,PREVPRICE" +
                              "&securities.columns=SECID,PREVLEGALCLOSEPRICE,FACEVALUE";
                    _logger.LogDebug("MOEX bond запрос: {Url}", url);

                    var json = await _httpClient.GetStringAsync(url);
                    using var doc = JsonDocument.Parse(json);

                    decimal pricePercent = 0;
                    decimal faceValue = 1000m; // По умолчанию для большинства рос. облигаций

                    // Приоритет: LAST > PREVPRICE (из marketdata) > PREVLEGALCLOSEPRICE (из securities)
                    var marketdata = doc.RootElement.GetProperty("marketdata");
                    var data = marketdata.GetProperty("data");

                    if (data.GetArrayLength() > 0)
                    {
                        var row = data[0];
                        if (TryGetNumber(row, 1, out var last))
                            pricePercent = last;
                        else if (TryGetNumber(row, 2, out var prev))
                            pricePercent = prev;
                    }

                    // Извлекаем FACEVALUE и fallback-цену из securities
                    var securities = doc.RootElement.GetProperty("securities");
                    var secData = securities.GetProperty("data");
                    if (secData.GetArrayLength() > 0)
                    {
                        var secRow = secData[0];
                        // FACEVALUE — 3-й столбец (индекс 2)
                        if (TryGetNumber(secRow, 2, out var face))
                            faceValue = face;

                        // Fallback: PREVLEGALCLOSEPRICE — 2-й столбец (индекс 1)
                        if (pricePercent <= 0 && TryGetNumber(secRow, 1, out var prevLegal))
                            pricePercent = prevLegal;
                    }

                    if (pricePercent > 0)
                    {
                        var absolutePrice = pricePercent * faceValue / 100m;
                        result[ticker] = absolutePrice;
                        _logger.LogDebug("MOEX Bond {Ticker}: {Percent}% × {Face} = {Absolute}",
                            ticker, pricePercent, faceValue, absolutePrice);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка загрузки цены облигации MOEX для {Ticker}", ticker);
                }
            }

            return result;
        }

        private async Task<Dictionary<string, decimal>> FetchPricesFromBoard(
            string engine, string market, string board, IEnumerable<string> tickers)
        {
            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (var ticker in tickers)
            {
                try
                {
                    var url = $"{_settings.BaseUrl}/engines/{engine}/markets/{market}/boards/{board}/securities/{ticker}.json?iss.meta=off&iss.only=marketdata,securities&marketdata.columns=SECID,LAST,PREVPRICE&securities.columns=SECID,PREVLEGALCLOSEPRICE";
                    _logger.LogDebug("MOEX запрос: {Url}", url);

                    var json = await _httpClient.GetStringAsync(url);
                    using var doc = JsonDocument.Parse(json);

                    decimal price = 0;

                    // Приоритет: LAST > PREVPRICE (из marketdata) > PREVLEGALCLOSEPRICE (из securities)
                    var marketdata = doc.RootElement.GetProperty("marketdata");
                    var data = marketdata.GetProperty("data");

                    if (data.GetArrayLength() > 0)
                    {
                        var row = data[0];
                        if (TryGetNumber(row, 1, out var last))
                            price = last;
                        else if (TryGetNumber(row, 2, out var prev))
                            price = prev;
                    }

                    // Fallback: когда биржа закрыта, marketdata пуст — берём цену закрытия из securities
                    if (price <= 0)
                    {
                        var securities = doc.RootElement.GetProperty("securities");
                        var secData = securities.GetProperty("data");
                        if (secData.GetArrayLength() > 0)
                        {
                            var secRow = secData[0];
                            if (TryGetNumber(secRow, 1, out var prevLegal))
                                price = prevLegal;
                        }
                    }

                    if (price > 0)
                    {
                        result[ticker] = price;
                        _logger.LogDebug("MOEX {Ticker}: {Price}", ticker, price);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка загрузки цены MOEX для {Ticker}", ticker);
                }
            }

            _logger.LogInformation("MOEX: загружено {Count} цен", result.Count);
            return result;
        }
    }
}
