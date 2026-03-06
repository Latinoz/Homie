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
            return await FetchPricesFromBoard("stock", "bonds", "TQCB", tickers);
        }

        private async Task<Dictionary<string, decimal>> FetchPricesFromBoard(
            string engine, string market, string board, IEnumerable<string> tickers)
        {
            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (var ticker in tickers)
            {
                try
                {
                    var url = $"{_settings.BaseUrl}/engines/{engine}/markets/{market}/boards/{board}/securities/{ticker}.json?iss.meta=off&iss.only=marketdata&marketdata.columns=SECID,LAST,PREVPRICE";
                    _logger.LogDebug("MOEX запрос: {Url}", url);

                    var json = await _httpClient.GetStringAsync(url);
                    using var doc = JsonDocument.Parse(json);

                    var marketdata = doc.RootElement.GetProperty("marketdata");
                    var data = marketdata.GetProperty("data");

                    if (data.GetArrayLength() > 0)
                    {
                        var row = data[0];
                        // LAST — последняя цена, PREVPRICE — цена закрытия предыдущего дня
                        decimal price = 0;
                        if (row[1].ValueKind == JsonValueKind.Number)
                            price = row[1].GetDecimal();
                        else if (row[2].ValueKind == JsonValueKind.Number)
                            price = row[2].GetDecimal();

                        if (price > 0)
                        {
                            result[ticker] = price;
                            _logger.LogDebug("MOEX {Ticker}: {Price}", ticker, price);
                        }
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
