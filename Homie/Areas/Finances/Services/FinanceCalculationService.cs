using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Homie.Areas.Finances.Models;
using Homie.Data.Models;

namespace Homie.Areas.Finances.Services
{
    public class FinanceCalculationService : IFinanceCalculationService
    {
        private readonly ApplicationDbContext _db;

        public FinanceCalculationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<FinanceDashboardViewModel> GetDashboardDataAsync(string userId)
        {
            var vm = new FinanceDashboardViewModel();

            // --- Текущие курсы ---
            var currencies = await _db.Currencies.Where(c => c.UserUid == userId).ToListAsync();
            var latestRates = new List<ExchangeRateModel>();

            foreach (var cur in currencies.Where(c => !c.IsBase))
            {
                var rate = await _db.ExchangeRates
                    .Where(r => r.CurrencyId == cur.Id && r.UserUid == userId)
                    .OrderByDescending(r => r.Date)
                    .FirstOrDefaultAsync();
                if (rate != null)
                    latestRates.Add(rate);
            }
            vm.LatestRates = latestRates;

            // Словарь быстрого доступа к курсам (currencyId → rate к RUB)
            var rateDict = latestRates.ToDictionary(r => r.CurrencyId, r => r.Rate);

            // Вспомогательная функция: получить курс к RUB
            decimal GetRubRate(int currencyId)
            {
                // Если это базовая валюта (RUB) — курс 1
                var cur = currencies.FirstOrDefault(c => c.Id == currencyId);
                if (cur != null && cur.IsBase) return 1m;
                return rateDict.TryGetValue(currencyId, out var r) ? r : 1m;
            }

            // --- Итого депозиты ---
            var deposits = await _db.Deposits
                .Where(d => d.UserUid == userId)
                .ToListAsync();
            vm.TotalDeposits = deposits.Sum(d => d.Amount * GetRubRate(d.CurrencyId));

            // --- Итого инвестиции ---
            var investPositions = await _db.InvestmentPositions
                .Where(ip => ip.UserUid == userId)
                .Include(ip => ip.Instrument)
                .ToListAsync();
            vm.TotalInvestments = investPositions.Sum(ip =>
                ip.Quantity * ip.CurrentPrice * GetRubRate(ip.Instrument?.CurrencyId ?? 0));

            // --- Итого крипто ---
            var cryptoAssets = await _db.CryptoAssets
                .Where(ca => ca.UserUid == userId)
                .ToListAsync();
            vm.TotalCrypto = cryptoAssets.Sum(ca =>
                ca.Quantity * ca.CurrentPrice * GetRubRate(ca.CurrencyId));

            // --- Итого драгметаллы ---
            var metals = await _db.PreciousMetals
                .Where(pm => pm.UserUid == userId)
                .ToListAsync();
            vm.TotalPreciousMetals = metals.Sum(pm => pm.TotalValueRub);

            // --- Общий капитал ---
            vm.TotalCapital = vm.TotalDeposits + vm.TotalInvestments + vm.TotalCrypto + vm.TotalPreciousMetals;

            // --- Инфляция ---
            var currentYear = DateTime.Now.Year;
            var inflation = await _db.Inflation
                .Where(i => i.UserUid == userId && i.Year == currentYear)
                .OrderByDescending(i => i.Month)
                .FirstOrDefaultAsync();
            vm.AccumulatedInflationPercent = inflation?.AccumulatedYearPercent ?? 0;
            vm.RealValueAfterInflation = vm.TotalCapital > 0 && vm.AccumulatedInflationPercent != 0
                ? vm.TotalCapital / (1 + vm.AccumulatedInflationPercent / 100m)
                : vm.TotalCapital;

            // --- Распределение активов (JSON для pie chart) ---
            var allocationLabels = new[] { "Депозиты", "Инвестиции", "Криптовалюта", "Драгметаллы" };
            var allocationValues = new[] { vm.TotalDeposits, vm.TotalInvestments, vm.TotalCrypto, vm.TotalPreciousMetals };
            vm.AssetAllocationJson = JsonSerializer.Serialize(new
            {
                labels = allocationLabels,
                values = allocationValues
            });

            // --- Динамика капитала ---
            vm.CapitalDynamicsJson = await GetPortfolioHistoryJsonAsync(userId, 12);

            // --- Дата последнего обновления цен ---
            vm.LastPriceUpdateDate = await _db.Instruments
                .Where(i => i.UserUid == userId && i.LastPriceDate != null)
                .MaxAsync(i => (DateTime?)i.LastPriceDate);

            // --- Последние операции ---
            vm.RecentOperations = await _db.FinanceOperations
                .Where(o => o.UserUid == userId)
                .Include(o => o.OperationType)
                .Include(o => o.Account)
                .OrderByDescending(o => o.Date)
                .Take(10)
                .ToListAsync();

            return vm;
        }

        public async Task<decimal> GetDepositBalanceAsync(int depositId, string userId)
        {
            // Сумма пополнений - снятий + начисленные проценты из журнала
            var operations = await _db.FinanceOperations
                .Where(o => o.UserUid == userId)
                .Include(o => o.OperationType)
                .Where(o => o.OperationType.Category == OperationCategory.Deposit)
                .ToListAsync();

            decimal balance = 0;
            foreach (var op in operations)
            {
                switch (op.OperationType.Name)
                {
                    case "Пополнение депозита":
                        balance += op.AmountInRub;
                        break;
                    case "Снятие с депозита":
                        balance -= op.AmountInRub;
                        break;
                    case "Начисление процентов":
                        balance += op.AmountInRub;
                        break;
                }
            }

            return balance;
        }

        public async Task<InvestmentMetrics> GetInvestmentMetricsAsync(int positionId, string userId)
        {
            var position = await _db.InvestmentPositions
                .Include(ip => ip.Instrument)
                .FirstOrDefaultAsync(ip => ip.Id == positionId && ip.UserUid == userId);

            if (position == null)
                return new InvestmentMetrics();

            var operations = await _db.FinanceOperations
                .Where(o => o.UserUid == userId && o.InstrumentId == position.InstrumentId)
                .Include(o => o.OperationType)
                .ToListAsync();

            var buyOps = operations.Where(o => o.OperationType.Name == "Покупка ценных бумаг").ToList();
            var sellOps = operations.Where(o => o.OperationType.Name == "Продажа ценных бумаг").ToList();
            var dividendOps = operations.Where(o => o.OperationType.Name == "Дивиденд" || o.OperationType.Name == "Купон").ToList();

            var totalBought = buyOps.Sum(o => o.Quantity ?? 0);
            var totalSold = sellOps.Sum(o => o.Quantity ?? 0);
            var quantity = totalBought - totalSold;

            var totalBoughtCost = buyOps.Sum(o => (o.Quantity ?? 0) * (o.Price ?? 0));
            var avgPrice = totalBought > 0 ? totalBoughtCost / totalBought : 0;

            var dividends = dividendOps.Sum(o => o.AmountInRub);

            var returnPct = avgPrice > 0
                ? (position.CurrentPrice - avgPrice) / avgPrice * 100
                : 0;

            return new InvestmentMetrics
            {
                QuantityFromJournal = quantity,
                AvgPriceFromJournal = avgPrice,
                DividendsFromJournal = dividends,
                ReturnPercent = returnPct,
                ValueInRub = quantity * position.CurrentPrice
            };
        }

        public async Task<string> GetPortfolioHistoryJsonAsync(string userId, int months = 12)
        {
            var startDate = DateTime.Today.AddMonths(-months);
            var labels = new List<string>();
            var values = new List<decimal>();

            for (int i = 0; i <= months; i++)
            {
                var date = startDate.AddMonths(i);
                var endOfMonth = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

                labels.Add(endOfMonth.ToString("MMM yyyy"));
                // Упрощённый расчёт — сумма AmountInRub операций до конца месяца
                var totalOps = await _db.FinanceOperations
                    .Where(o => o.UserUid == userId && o.Date <= endOfMonth)
                    .SumAsync(o => o.AmountInRub);
                values.Add(totalOps);
            }

            return JsonSerializer.Serialize(new { labels, values });
        }

        public async Task<string> GetInstrumentPriceChartJsonAsync(int instrumentId, string userId, int months = 12)
        {
            var startDate = DateTime.Today.AddMonths(-months);

            var priceHistory = await _db.PriceHistory
                .Where(ph => ph.InstrumentId == instrumentId && ph.UserUid == userId && ph.Date >= startDate)
                .OrderBy(ph => ph.Date)
                .ToListAsync();

            var labels = priceHistory.Select(ph => ph.Date.ToString("dd.MM.yyyy")).ToList();
            var values = priceHistory.Select(ph => ph.Price).ToList();

            return JsonSerializer.Serialize(new { labels, values });
        }
    }
}
