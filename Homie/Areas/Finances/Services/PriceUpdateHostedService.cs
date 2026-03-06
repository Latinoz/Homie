using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Homie.Areas.Finances.Models;

namespace Homie.Areas.Finances.Services
{
    /// <summary>Фоновый сервис ежедневного обновления курсов и цен</summary>
    public class PriceUpdateHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PriceUpdateHostedService> _logger;
        private readonly FinancesSettings _settings;

        public PriceUpdateHostedService(
            IServiceProvider serviceProvider,
            IOptions<FinancesSettings> settings,
            ILogger<PriceUpdateHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.EnableBackgroundPriceUpdate)
            {
                _logger.LogInformation("Фоновое обновление цен отключено в настройках");
                return;
            }

            _logger.LogInformation("PriceUpdateHostedService запущен. Обновление в {Time} UTC",
                _settings.PriceUpdateTimeUtc);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    var targetTime = TimeSpan.Parse(_settings.PriceUpdateTimeUtc);
                    var nextRun = now.Date.Add(targetTime);

                    if (now > nextRun)
                        nextRun = nextRun.AddDays(1);

                    var delay = nextRun - now;
                    _logger.LogDebug("Следующее обновление цен через {Delay}", delay);

                    await Task.Delay(delay, stoppingToken);

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    await RunUpdateAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в PriceUpdateHostedService");
                    // Ждём 1 час перед повторной попыткой
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task RunUpdateAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Запуск ежедневного обновления цен...");

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Data.Models.ApplicationDbContext>();
            var orchestrator = scope.ServiceProvider.GetRequiredService<IPriceUpdateOrchestrator>();
            var cbr = scope.ServiceProvider.GetRequiredService<ICbrExchangeRateService>();

            // --- 1. Обновляем курсы валют ЦБ ---
            try
            {
                var rates = await cbr.FetchCurrencyRatesAsync(DateTime.Today);
                var users = await db.Currencies.Select(c => c.UserUid).Distinct().ToListAsync(stoppingToken);

                foreach (var userId in users.Where(u => u != null))
                {
                    var userCurrencies = await db.Currencies
                        .Where(c => c.UserUid == userId && !c.IsBase && c.CbrCode != null)
                        .ToListAsync(stoppingToken);

                    foreach (var cur in userCurrencies)
                    {
                        if (rates.TryGetValue(cur.Code, out var rate))
                        {
                            var existing = await db.ExchangeRates
                                .FirstOrDefaultAsync(r => r.CurrencyId == cur.Id
                                                          && r.Date.Date == DateTime.Today
                                                          && r.UserUid == userId, stoppingToken);
                            if (existing != null)
                            {
                                existing.Rate = rate;
                                existing.Source = PriceSource.CbrAuto;
                            }
                            else
                            {
                                db.ExchangeRates.Add(new ExchangeRateModel
                                {
                                    CurrencyId = cur.Id,
                                    Date = DateTime.Today,
                                    Rate = rate,
                                    Source = PriceSource.CbrAuto,
                                    UserUid = userId
                                });
                            }
                        }
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Курсы валют ЦБ обновлены");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении курсов ЦБ");
            }

            // --- 2. Обновляем цены для каждого пользователя ---
            try
            {
                var allUsers = await db.Instruments
                    .Select(i => i.UserUid)
                    .Distinct()
                    .ToListAsync(stoppingToken);

                foreach (var userId in allUsers.Where(u => u != null))
                {
                    try
                    {
                        var result = await orchestrator.UpdateAllPricesAsync(userId);
                        _logger.LogInformation("Цены для {UserId}: {Success} ok, {Errors} ошибок",
                            userId, result.SuccessCount, result.ErrorCount);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка обновления цен для {UserId}", userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении цен");
            }

            _logger.LogInformation("Ежедневное обновление цен завершено");
        }
    }
}
