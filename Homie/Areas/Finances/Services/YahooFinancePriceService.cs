using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Homie.Areas.Finances.Models;

namespace Homie.Areas.Finances.Services
{
    public class YahooFinancePriceService : IYahooFinancePriceService
    {
        private readonly HttpClient _httpClient;
        private readonly YahooFinanceSettings _settings;
        private readonly ILogger<YahooFinancePriceService> _logger;

        public YahooFinancePriceService(HttpClient httpClient, IOptions<FinancesSettings> options,
            ILogger<YahooFinancePriceService> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value.YahooFinance;
            _logger = logger;
        }

        public async Task<Dictionary<string, decimal>> FetchPricesAsync(IEnumerable<string> tickers)
        {
            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (var ticker in tickers)
            {
                try
                {
                    var url = $"{_settings.BaseUrl}/{ticker}?interval=1d&range=1d";
                    _logger.LogDebug("Yahoo Finance запрос: {Url}", url);

                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("User-Agent", "Mozilla/5.0");

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    var chart = doc.RootElement.GetProperty("chart");
                    var results = chart.GetProperty("result");

                    if (results.GetArrayLength() > 0)
                    {
                        var meta = results[0].GetProperty("meta");
                        if (meta.TryGetProperty("regularMarketPrice", out var priceEl))
                        {
                            var price = priceEl.GetDecimal();
                            if (price > 0)
                            {
                                result[ticker] = price;
                                _logger.LogDebug("Yahoo {Ticker}: {Price}", ticker, price);
                            }
                        }
                    }

                    // Задержка между запросами для соблюдения rate limit
                    await Task.Delay(_settings.RequestDelayMs);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка загрузки цены Yahoo для {Ticker}", ticker);
                }
            }

            _logger.LogInformation("Yahoo Finance: загружено {Count} цен", result.Count);
            return result;
        }
    }
}
