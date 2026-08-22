using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Homie.Areas.Finances.Models;

namespace Homie.Areas.Finances.Services
{
    public class CoinGeckoPriceService : ICoinGeckoPriceService
    {
        private readonly HttpClient _httpClient;
        private readonly CoinGeckoSettings _settings;
        private readonly ILogger<CoinGeckoPriceService> _logger;

        public CoinGeckoPriceService(HttpClient httpClient, IOptions<FinancesSettings> options,
            ILogger<CoinGeckoPriceService> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value.CoinGecko;
            _logger = logger;
        }

        public async Task<Dictionary<string, CryptoPriceDto>> FetchPricesAsync(IEnumerable<string> coinGeckoIds)
        {
            var result = new Dictionary<string, CryptoPriceDto>(StringComparer.OrdinalIgnoreCase);
            var idList = coinGeckoIds.ToList();

            if (!idList.Any())
                return result;

            try
            {
                // CoinGecko позволяет batch-запрос до 250 id
                var ids = string.Join(",", idList);
                var url = $"{_settings.BaseUrl}/simple/price?ids={ids}&vs_currencies=usd,rub";
                _logger.LogDebug("CoinGecko запрос: {Url}", url);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; HomieApp/1.0)");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                foreach (var id in idList)
                {
                    if (doc.RootElement.TryGetProperty(id.ToLowerInvariant(), out var coinEl))
                    {
                        var dto = new CryptoPriceDto();

                        if (coinEl.TryGetProperty("usd", out var usdEl))
                            dto.Usd = usdEl.GetDecimal();

                        if (coinEl.TryGetProperty("rub", out var rubEl))
                            dto.Rub = rubEl.GetDecimal();

                        result[id] = dto;
                        _logger.LogDebug("CoinGecko {Id}: USD={Usd}, RUB={Rub}", id, dto.Usd, dto.Rub);
                    }
                }

                _logger.LogInformation("CoinGecko: загружено {Count} цен", result.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки цен CoinGecko");
            }

            return result;
        }
    }
}
