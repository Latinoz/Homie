using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Homie.Areas.Finances.Models;

namespace Homie.Areas.Finances.Services
{
    public class CbrExchangeRateService : ICbrExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CbrExchangeRateService> _logger;

        public CbrExchangeRateService(HttpClient httpClient, ILogger<CbrExchangeRateService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Dictionary<string, decimal>> FetchCurrencyRatesAsync(DateTime date)
        {
            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var dateStr = date.ToString("dd/MM/yyyy");
                var url = $"https://cbr.ru/scripts/XML_daily.asp?date_req={dateStr}";
                _logger.LogDebug("Запрос курсов ЦБ: {Url}", url);

                var xml = await _httpClient.GetStringAsync(url);
                var doc = XDocument.Parse(xml);

                foreach (var valute in doc.Descendants("Valute"))
                {
                    var charCode = valute.Element("CharCode")?.Value;
                    var nominalStr = valute.Element("Nominal")?.Value;
                    var valueStr = valute.Element("Value")?.Value;

                    if (string.IsNullOrEmpty(charCode) || string.IsNullOrEmpty(valueStr))
                        continue;

                    // ЦБ использует запятую как десятичный разделитель
                    if (decimal.TryParse(valueStr.Replace(",", "."), NumberStyles.Any,
                            CultureInfo.InvariantCulture, out var value) &&
                        decimal.TryParse(nominalStr?.Replace(",", ".") ?? "1", NumberStyles.Any,
                            CultureInfo.InvariantCulture, out var nominal))
                    {
                        // Курс за 1 единицу валюты
                        result[charCode] = value / nominal;
                    }
                }

                _logger.LogInformation("Загружено {Count} курсов валют ЦБ на {Date}", result.Count, dateStr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки курсов ЦБ");
            }

            return result;
        }

        public async Task<Dictionary<string, decimal>> FetchMetalPricesAsync(DateTime date)
        {
            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var dateFrom = date.ToString("dd/MM/yyyy");
                var dateTo = date.ToString("dd/MM/yyyy");
                var url = $"https://cbr.ru/scripts/xml_metall.asp?date_req1={dateFrom}&date_req2={dateTo}";
                _logger.LogDebug("Запрос цен металлов ЦБ: {Url}", url);

                var xml = await _httpClient.GetStringAsync(url);
                var doc = XDocument.Parse(xml);

                // Маппинг кодов ЦБ на наши имена
                var codeToName = new Dictionary<string, string>
                {
                    { "1", "Gold" },
                    { "2", "Silver" },
                    { "3", "Platinum" },
                    { "4", "Palladium" }
                };

                foreach (var record in doc.Descendants("Record"))
                {
                    var code = record.Attribute("Code")?.Value;
                    var buyStr = record.Element("Buy")?.Value;
                    var sellStr = record.Element("Sell")?.Value;

                    if (string.IsNullOrEmpty(code) || !codeToName.ContainsKey(code))
                        continue;

                    // Используем среднее между покупкой и продажей как ориентир
                    if (decimal.TryParse(buyStr?.Replace(",", "."), NumberStyles.Any,
                            CultureInfo.InvariantCulture, out var buy) &&
                        decimal.TryParse(sellStr?.Replace(",", "."), NumberStyles.Any,
                            CultureInfo.InvariantCulture, out var sell))
                    {
                        result[codeToName[code]] = (buy + sell) / 2;
                    }
                    else if (decimal.TryParse(buyStr?.Replace(",", "."), NumberStyles.Any,
                                 CultureInfo.InvariantCulture, out var buyOnly))
                    {
                        result[codeToName[code]] = buyOnly;
                    }
                }

                _logger.LogInformation("Загружено {Count} цен металлов ЦБ на {Date}", result.Count, dateFrom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки цен металлов ЦБ");
            }

            return result;
        }
    }
}
