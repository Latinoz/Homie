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
            foreach (var d in deposits)
            {
                d.BalanceFromJournal = await GetDepositBalanceAsync(d.Id, userId);
            }
            vm.TotalDeposits = deposits.Sum(d => d.BalanceFromJournal * GetRubRate(d.CurrencyId));

            // --- Итого инвестиции ---
            var investPositions = await _db.InvestmentPositions
                .Where(ip => ip.UserUid == userId)
                .Include(ip => ip.Instrument)
                .ToListAsync();
            vm.TotalInvestments = investPositions
                .Where(ip => ip.Quantity > 0)
                .Sum(ip => ip.Quantity * ip.CurrentPrice * GetRubRate(ip.Instrument?.CurrencyId ?? 0));

            // --- Кэш-баланс брокерских счётов (дивиденды, купоны, комиссии, налоги) ---
            // Покупка/продажа ценных бумаг уже отражены в позициях — их сюда не включаем.
            var brokerCashOps = await _db.FinanceOperations
                .Where(o => o.UserUid == userId)
                .Include(o => o.OperationType)
                .Include(o => o.Account)
                .Where(o => o.Account.AccountType == AccountType.Broker
                         && o.OperationType.Category == OperationCategory.Investment
                         && o.OperationType.Name != "Покупка ценных бумаг"
                         && o.OperationType.Name != "Продажа ценных бумаг")
                .ToListAsync();

            foreach (var op in brokerCashOps)
            {
                switch (op.OperationType.Name)
                {
                    case "Дивиденд":
                    case "Купон":
                        vm.TotalInvestments += op.AmountInRub;
                        break;
                    case "Комиссия":
                    case "Налог":
                        vm.TotalInvestments -= op.AmountInRub;
                        break;
                }
            }

            // --- Итого крипто ---
            var cryptoAssets = await _db.CryptoAssets
                .Where(ca => ca.UserUid == userId)
                .Include(ca => ca.Currency)
                .ToListAsync();

            var cryptoOps = await _db.FinanceOperations
                .Where(o => o.UserUid == userId)
                .Include(o => o.OperationType)
                .Where(o => o.OperationType.Category == OperationCategory.Crypto
                         && (o.OperationType.Name == "Покупка криптовалюты"
                          || o.OperationType.Name == "Продажа криптовалюты"))
                .ToListAsync();

            vm.TotalCrypto = cryptoAssets.Sum(ca =>
            {
                var journalQty = cryptoOps
                    .Where(o => o.InstrumentId == ca.InstrumentId && o.AccountId == ca.AccountId)
                    .Sum(o => o.OperationType.Name == "Покупка криптовалюты"
                        ? (o.Quantity ?? 0)
                        : -(o.Quantity ?? 0));
                var effectiveQty = journalQty > 0 ? journalQty : ca.Quantity;
                return Math.Max(effectiveQty, 0) * ca.CurrentPrice * GetRubRate(ca.CurrencyId ?? 0);
            });

            // --- Итого драгметаллы ---
            var metals = await _db.PreciousMetals
                .Where(pm => pm.UserUid == userId)
                .ToListAsync();

            // Операции покупки/продажи драгметаллов из журнала
            var pmOps = await _db.FinanceOperations
                .Where(o => o.UserUid == userId && o.InstrumentId != null)
                .Include(o => o.OperationType)
                .Where(o => o.OperationType.Name == "Покупка драгметалла"
                          || o.OperationType.Name == "Продажа драгметалла")
                .ToListAsync();

            decimal totalPM = 0;
            foreach (var m in metals)
            {
                int effectiveQty = m.Quantity;
                if (m.InstrumentId.HasValue)
                {
                    var bought = pmOps
                        .Where(o => o.InstrumentId == m.InstrumentId && o.OperationType.Name == "Покупка драгметалла")
                        .Sum(o => o.Quantity ?? 0);
                    var sold = pmOps
                        .Where(o => o.InstrumentId == m.InstrumentId && o.OperationType.Name == "Продажа драгметалла")
                        .Sum(o => o.Quantity ?? 0);
                    effectiveQty = Math.Max(m.Quantity + (int)(bought - sold), 0);
                }
                totalPM += effectiveQty * m.WeightGrams * m.CurrentPricePerGram;
            }
            vm.TotalPreciousMetals = totalPM;

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
                datasets = new[] {
                    new {
                        data = allocationValues,
                        backgroundColor = new[] { "#4e73df", "#1cc88a", "#f6c23e", "#e74a3b" }
                    }
                }
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
            // Найти депозит, чтобы получить его AccountId
            var deposit = await _db.Deposits
                .FirstOrDefaultAsync(d => d.Id == depositId && d.UserUid == userId);
            if (deposit == null) return 0;

            // Начальная сумма + операции из журнала (пополнения, снятия, проценты)
            var operations = await _db.FinanceOperations
                .Where(o => o.UserUid == userId && o.AccountId == deposit.AccountId)
                .Include(o => o.OperationType)
                .Where(o => o.OperationType.Category == OperationCategory.Deposit)
                .ToListAsync();

            decimal balance = deposit.Amount;
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

        public async Task<CryptoMetrics> GetCryptoMetricsAsync(int assetId, string userId)
        {
            var asset = await _db.CryptoAssets
                .Include(ca => ca.Currency)
                .FirstOrDefaultAsync(ca => ca.Id == assetId && ca.UserUid == userId);
            if (asset == null) return new CryptoMetrics();

            var operations = await _db.FinanceOperations
                .Where(o => o.UserUid == userId
                         && o.InstrumentId == asset.InstrumentId
                         && o.AccountId == asset.AccountId)
                .Include(o => o.OperationType)
                .Where(o => o.OperationType.Category == OperationCategory.Crypto)
                .ToListAsync();

            var buyOps = operations.Where(o => o.OperationType.Name == "Покупка криптовалюты").ToList();
            var sellOps = operations.Where(o => o.OperationType.Name == "Продажа криптовалюты").ToList();

            var totalBought = buyOps.Sum(o => o.Quantity ?? 0);
            var totalSold = sellOps.Sum(o => o.Quantity ?? 0);
            var quantity = totalBought - totalSold;

            var valueInCurrency = quantity * asset.CurrentPrice;

            // Получить курс валюты актива к RUB
            var currencies = await _db.Currencies.Where(c => c.UserUid == userId).ToListAsync();
            var isBase = asset.Currency != null && asset.Currency.IsBase;
            decimal rubRate = 1m;
            if (!isBase)
            {
                var rate = await _db.ExchangeRates
                    .Where(r => r.CurrencyId == asset.CurrencyId && r.UserUid == userId)
                    .OrderByDescending(r => r.Date)
                    .FirstOrDefaultAsync();
                rubRate = rate?.Rate ?? 1m;
            }

            // Для ValueInUsd: найти USD курс
            var usdCurrency = currencies.FirstOrDefault(c => c.Code == "USD");
            decimal usdRate = 1m;
            if (usdCurrency != null && !usdCurrency.IsBase)
            {
                var usdExRate = await _db.ExchangeRates
                    .Where(r => r.CurrencyId == usdCurrency.Id && r.UserUid == userId)
                    .OrderByDescending(r => r.Date)
                    .FirstOrDefaultAsync();
                usdRate = usdExRate?.Rate ?? 1m;
            }

            var valueInRub = valueInCurrency * rubRate;
            var valueInUsd = usdRate > 0 ? valueInRub / usdRate : 0;

            return new CryptoMetrics
            {
                QuantityFromJournal = quantity,
                ValueInUsd = valueInUsd,
                ValueInRub = valueInRub
            };
        }

        public async Task<int> EnsureCryptoWalletAccountsAsync(string userId)
        {
            var cryptoWallets = await _db.Wallets
                .Where(w => w.UserUid == userId && w.Type == WalletType.Crypto)
                .ToListAsync();

            var linkedWalletIds = await _db.FinanceAccounts
                .Where(a => a.UserUid == userId && a.AccountType == AccountType.Wallet && a.WalletId != null)
                .Select(a => a.WalletId.Value)
                .ToListAsync();

            var currencies = await _db.Currencies
                .Where(c => c.UserUid == userId)
                .OrderBy(c => c.Id)
                .ToListAsync();

            // Базовая валюта → RUB → любая валюта пользователя
            var fallbackCurrency = currencies.FirstOrDefault(c => c.IsBase)
                ?? currencies.FirstOrDefault(c => c.Code == "RUB")
                ?? currencies.FirstOrDefault();

            int created = 0;
            foreach (var wallet in cryptoWallets)
            {
                if (linkedWalletIds.Contains(wallet.Id)) continue;

                int? currencyId = wallet.CurrencyId ?? fallbackCurrency?.Id;
                if (currencyId == null) continue; // валют нет вообще — счёт создать нельзя (FK NOT NULL)

                _db.FinanceAccounts.Add(new AccountModel
                {
                    Name = wallet.Name,
                    AccountType = AccountType.Wallet,
                    WalletId = wallet.Id,
                    CurrencyId = currencyId.Value,
                    UserUid = userId,
                    IsActive = true
                });
                created++;
            }

            if (created > 0)
                await _db.SaveChangesAsync();

            return created;
        }

        public async Task<List<WalletCryptoHolding>> GetWalletCryptoHoldingsAsync(string userId)
        {
            var result = new List<WalletCryptoHolding>();

            var walletIds = await _db.Wallets
                .Where(w => w.UserUid == userId && w.Type == WalletType.Crypto)
                .Select(w => w.Id)
                .ToListAsync();
            if (!walletIds.Any()) return result;

            var accounts = await _db.FinanceAccounts
                .Where(a => a.UserUid == userId && a.AccountType == AccountType.Wallet
                         && a.WalletId != null && walletIds.Contains(a.WalletId.Value))
                .ToListAsync();
            var accountIds = accounts.Select(a => a.Id).ToList();
            if (!accountIds.Any()) return result;

            var assets = await _db.CryptoAssets
                .Where(ca => ca.UserUid == userId && ca.AccountId.HasValue && accountIds.Contains(ca.AccountId.Value))
                .Include(ca => ca.Instrument)
                .ToListAsync();

            var cryptoOps = await _db.FinanceOperations
                .Where(o => o.UserUid == userId)
                .Include(o => o.OperationType)
                .Where(o => o.OperationType.Name == "Покупка криптовалюты"
                         || o.OperationType.Name == "Продажа криптовалюты")
                .ToListAsync();

            var currencies = await _db.Currencies.Where(c => c.UserUid == userId).ToListAsync();
            var latestRates = await _db.ExchangeRates
                .Where(r => r.UserUid == userId)
                .ToListAsync();
            var rateDict = latestRates
                .GroupBy(r => r.CurrencyId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.Date).First().Rate);

            decimal GetRubRate(int currencyId)
            {
                var cur = currencies.FirstOrDefault(c => c.Id == currencyId);
                if (cur != null && cur.IsBase) return 1m;
                return rateDict.TryGetValue(currencyId, out var r) ? r : 1m;
            }

            foreach (var asset in assets)
            {
                var journalQty = cryptoOps
                    .Where(o => o.InstrumentId == asset.InstrumentId && o.AccountId == asset.AccountId)
                    .Sum(o => o.OperationType.Name == "Покупка криптовалюты"
                        ? (o.Quantity ?? 0)
                        : -(o.Quantity ?? 0));
                var effectiveQty = journalQty > 0 ? journalQty : asset.Quantity;
                if (effectiveQty <= 0) continue;

                var account = accounts.FirstOrDefault(a => a.Id == asset.AccountId.Value);
                result.Add(new WalletCryptoHolding
                {
                    WalletId = account?.WalletId ?? 0,
                    Ticker = asset.Ticker,
                    InstrumentName = asset.Instrument?.Name,
                    Quantity = effectiveQty,
                    ValueInRub = effectiveQty * asset.CurrentPrice * GetRubRate(asset.CurrencyId ?? 0)
                });
            }

            return result;
        }

        public async Task ApplyCryptoOperationAsync(OperationModel operation, string userId)
        {
            if (operation.InstrumentId == null) return;

            var opType = operation.OperationType
                ?? await _db.OperationTypes.FindAsync(operation.OperationTypeId);
            if (opType == null) return;

            bool isBuy = opType.Name == "Покупка криптовалюты";
            bool isSell = opType.Name == "Продажа криптовалюты";
            if (!isBuy && !isSell) return;

            decimal opQty = operation.Quantity ?? 0;
            if (opQty == 0) return;

            decimal opPrice = operation.Price
                ?? (opQty > 0 ? operation.AmountInRub / opQty : 0);

            var asset = await _db.CryptoAssets
                .FirstOrDefaultAsync(ca => ca.InstrumentId == operation.InstrumentId
                                        && ca.AccountId == operation.AccountId
                                        && ca.UserUid == userId);

            if (asset == null)
            {
                if (isSell) return;

                var instrument = await _db.Instruments.FindAsync(operation.InstrumentId.Value);
                asset = new CryptoAssetModel
                {
                    InstrumentId = operation.InstrumentId.Value,
                    AccountId = operation.AccountId,
                    UserUid = userId,
                    Ticker = instrument?.Code ?? "",
                    CurrencyId = operation.CurrencyId,
                    Quantity = opQty,
                    AvgPurchasePrice = opPrice,
                    CurrentPrice = opPrice,
                    IsAutoUpdateEnabled = true
                };
                _db.CryptoAssets.Add(asset);
            }
            else
            {
                if (isBuy)
                {
                    var totalQty = asset.Quantity + opQty;
                    if (totalQty > 0)
                        asset.AvgPurchasePrice =
                            (asset.AvgPurchasePrice * asset.Quantity + opPrice * opQty) / totalQty;
                    asset.Quantity = totalQty;
                }
                else
                {
                    asset.Quantity = Math.Max(asset.Quantity - opQty, 0);
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task RevertCryptoOperationAsync(OperationModel operation, string userId)
        {
            if (operation.InstrumentId == null) return;

            var opType = operation.OperationType
                ?? await _db.OperationTypes.FindAsync(operation.OperationTypeId);
            if (opType == null) return;

            bool isBuy = opType.Name == "Покупка криптовалюты";
            bool isSell = opType.Name == "Продажа криптовалюты";
            if (!isBuy && !isSell) return;

            decimal opQty = operation.Quantity ?? 0;
            if (opQty == 0) return;

            decimal opPrice = operation.Price
                ?? (opQty > 0 ? operation.AmountInRub / opQty : 0);

            var asset = await _db.CryptoAssets
                .FirstOrDefaultAsync(ca => ca.InstrumentId == operation.InstrumentId
                                        && ca.AccountId == operation.AccountId
                                        && ca.UserUid == userId);
            if (asset == null) return;

            if (isBuy)
            {
                var newQty = asset.Quantity - opQty;
                if (newQty > 0 && asset.Quantity > 0)
                {
                    asset.AvgPurchasePrice =
                        (asset.AvgPurchasePrice * asset.Quantity - opPrice * opQty) / newQty;
                    if (asset.AvgPurchasePrice < 0)
                        asset.AvgPurchasePrice = 0;
                }
                asset.Quantity = Math.Max(newQty, 0);
            }
            else
            {
                asset.Quantity += opQty;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<int> SyncCryptoAssetsFromJournalAsync(string userId)
        {
            var operations = await _db.FinanceOperations
                .Where(o => o.UserUid == userId)
                .Include(o => o.OperationType)
                .Where(o => o.OperationType.Name == "Покупка криптовалюты"
                         || o.OperationType.Name == "Продажа криптовалюты")
                .ToListAsync();

            var groups = operations
                .Where(o => o.InstrumentId.HasValue)
                .GroupBy(o => new { o.InstrumentId, o.AccountId });

            int updated = 0;

            foreach (var group in groups)
            {
                var buyOps = group.Where(o => o.OperationType.Name == "Покупка криптовалюты").ToList();
                var sellOps = group.Where(o => o.OperationType.Name == "Продажа криптовалюты").ToList();

                var totalBought = buyOps.Sum(o => o.Quantity ?? 0);
                var totalSold = sellOps.Sum(o => o.Quantity ?? 0);
                var quantity = totalBought - totalSold;

                var totalBoughtCost = buyOps.Sum(o =>
                {
                    var qty = o.Quantity ?? 0;
                    var price = o.Price ?? (qty > 0 ? o.AmountInRub / qty : 0);
                    return qty * price;
                });
                var avgPrice = totalBought > 0 ? totalBoughtCost / totalBought : 0;

                var asset = await _db.CryptoAssets
                    .FirstOrDefaultAsync(ca => ca.InstrumentId == group.Key.InstrumentId
                                            && ca.AccountId == group.Key.AccountId
                                            && ca.UserUid == userId);

                if (asset == null)
                {
                    if (quantity <= 0) continue;
                    var instrument = await _db.Instruments.FindAsync(group.Key.InstrumentId.Value);
                    asset = new CryptoAssetModel
                    {
                        InstrumentId = group.Key.InstrumentId.Value,
                        AccountId = group.Key.AccountId,
                        UserUid = userId,
                        Ticker = instrument?.Code ?? "",
                        CurrencyId = buyOps.First().CurrencyId,
                        Quantity = quantity,
                        AvgPurchasePrice = avgPrice,
                        CurrentPrice = avgPrice,
                        IsAutoUpdateEnabled = true
                    };
                    _db.CryptoAssets.Add(asset);
                }
                else
                {
                    asset.Quantity = Math.Max(quantity, 0);
                    asset.AvgPurchasePrice = avgPrice;
                    if (asset.CurrentPrice == 0)
                        asset.CurrentPrice = avgPrice;
                }

                updated++;
            }

            await _db.SaveChangesAsync();
            return updated;
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

            return JsonSerializer.Serialize(new
            {
                labels,
                datasets = new[] {
                    new {
                        label = "Капитал",
                        data = values,
                        borderColor = "#4e73df",
                        backgroundColor = "rgba(78, 115, 223, 0.1)",
                        fill = true,
                        tension = 0.3
                    }
                }
            });
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

            return JsonSerializer.Serialize(new
            {
                labels,
                datasets = new[] {
                    new {
                        label = "Цена",
                        data = values,
                        borderColor = "#1cc88a",
                        fill = false,
                        tension = 0.1
                    }
                }
            });
        }

        public async Task ApplyOperationToPositionAsync(OperationModel operation, string userId)
        {
            if (operation.InstrumentId == null) return;

            var opType = operation.OperationType
                ?? await _db.OperationTypes.FindAsync(operation.OperationTypeId);
            if (opType == null) return;

            bool isBuy = opType.Name == "Покупка ценных бумаг";
            bool isSell = opType.Name == "Продажа ценных бумаг";
            if (!isBuy && !isSell) return;

            decimal opQty = operation.Quantity ?? 0;
            if (opQty == 0) return;

            // Если Price не заполнена, вычислить из AmountInRub / Quantity
            decimal opPrice = operation.Price
                ?? (opQty > 0 ? operation.AmountInRub / opQty : 0);

            var position = await _db.InvestmentPositions
                .FirstOrDefaultAsync(ip => ip.InstrumentId == operation.InstrumentId
                                        && ip.AccountId == operation.AccountId
                                        && ip.UserUid == userId);

            if (position == null)
            {
                if (isSell) return;
                position = new InvestmentPositionModel
                {
                    InstrumentId = operation.InstrumentId.Value,
                    AccountId = operation.AccountId,
                    UserUid = userId,
                    Quantity = opQty,
                    AvgPurchasePrice = opPrice,
                    CurrentPrice = opPrice,
                    IsAutoUpdateEnabled = true
                };
                _db.InvestmentPositions.Add(position);
            }
            else
            {
                if (isBuy)
                {
                    var totalQty = position.Quantity + opQty;
                    if (totalQty > 0)
                        position.AvgPurchasePrice =
                            (position.AvgPurchasePrice * position.Quantity + opPrice * opQty) / totalQty;
                    position.Quantity = totalQty;
                    // Обновить CurrentPrice ценой покупки (до автообновления с биржи)
                    position.CurrentPrice = opPrice;
                }
                else // isSell
                {
                    position.Quantity -= opQty;
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task RevertOperationFromPositionAsync(OperationModel operation, string userId)
        {
            if (operation.InstrumentId == null) return;

            var opType = operation.OperationType
                ?? await _db.OperationTypes.FindAsync(operation.OperationTypeId);
            if (opType == null) return;

            bool isBuy = opType.Name == "Покупка ценных бумаг";
            bool isSell = opType.Name == "Продажа ценных бумаг";
            if (!isBuy && !isSell) return;

            decimal opQty = operation.Quantity ?? 0;
            if (opQty == 0) return;

            decimal opPrice = operation.Price
                ?? (opQty > 0 ? operation.AmountInRub / opQty : 0);

            var position = await _db.InvestmentPositions
                .FirstOrDefaultAsync(ip => ip.InstrumentId == operation.InstrumentId
                                        && ip.AccountId == operation.AccountId
                                        && ip.UserUid == userId);

            if (position == null) return;

            if (isBuy)
            {
                var newQty = position.Quantity - opQty;
                if (newQty > 0 && position.Quantity > 0)
                {
                    position.AvgPurchasePrice =
                        (position.AvgPurchasePrice * position.Quantity - opPrice * opQty) / newQty;
                    if (position.AvgPurchasePrice < 0)
                        position.AvgPurchasePrice = 0;
                }
                position.Quantity = Math.Max(newQty, 0);
            }
            else // isSell — откатить продажу = вернуть количество
            {
                position.Quantity += opQty;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<int> SyncAllPositionsFromJournalAsync(string userId)
        {
            // Все операции покупки/продажи ценных бумаг
            var operations = await _db.FinanceOperations
                .Where(o => o.UserUid == userId)
                .Include(o => o.OperationType)
                .Where(o => o.OperationType.Name == "Покупка ценных бумаг"
                          || o.OperationType.Name == "Продажа ценных бумаг")
                .ToListAsync();

            // Группируем по (InstrumentId, AccountId)
            var groups = operations
                .Where(o => o.InstrumentId.HasValue)
                .GroupBy(o => new { o.InstrumentId, o.AccountId });

            int updated = 0;

            foreach (var group in groups)
            {
                var buyOps = group.Where(o => o.OperationType.Name == "Покупка ценных бумаг").ToList();
                var sellOps = group.Where(o => o.OperationType.Name == "Продажа ценных бумаг").ToList();

                var totalBought = buyOps.Sum(o => o.Quantity ?? 0);
                var totalSold = sellOps.Sum(o => o.Quantity ?? 0);
                var quantity = totalBought - totalSold;

                // Средневзвешенная цена покупки (если Price пустая — берём AmountInRub / Quantity)
                var totalBoughtCost = buyOps.Sum(o =>
                {
                    var qty = o.Quantity ?? 0;
                    var price = o.Price ?? (qty > 0 ? o.AmountInRub / qty : 0);
                    return qty * price;
                });
                var avgPrice = totalBought > 0 ? totalBoughtCost / totalBought : 0;

                var position = await _db.InvestmentPositions
                    .FirstOrDefaultAsync(ip => ip.InstrumentId == group.Key.InstrumentId
                                            && ip.AccountId == group.Key.AccountId
                                            && ip.UserUid == userId);

                if (position == null)
                {
                    if (quantity <= 0) continue;
                    position = new InvestmentPositionModel
                    {
                        InstrumentId = group.Key.InstrumentId.Value,
                        AccountId = group.Key.AccountId,
                        UserUid = userId,
                        Quantity = quantity,
                        AvgPurchasePrice = avgPrice,
                        CurrentPrice = avgPrice,
                        IsAutoUpdateEnabled = true
                    };
                    _db.InvestmentPositions.Add(position);
                }
                else
                {
                    position.Quantity = Math.Max(quantity, 0);
                    position.AvgPurchasePrice = avgPrice;
                    if (position.CurrentPrice == 0)
                        position.CurrentPrice = avgPrice;
                }

                updated++;
            }

            await _db.SaveChangesAsync();
            return updated;
        }

        public async Task ApplyPreciousMetalOperationAsync(OperationModel operation, string userId)
        {
            // Драгметаллы вычисляются из журнала на лету в GetDashboardDataAsync.
            // Этот метод оставлен для совместимости интерфейса.
            await Task.CompletedTask;
        }

        public async Task RevertPreciousMetalOperationAsync(OperationModel operation, string userId)
        {
            // Драгметаллы вычисляются из журнала на лету в GetDashboardDataAsync.
            // Этот метод оставлен для совместимости интерфейса.
            await Task.CompletedTask;
        }

        public async Task<int> SyncPreciousMetalPositionsFromJournalAsync(string userId)
        {
            // Драгметаллы теперь вычисляются из журнала на лету.
            await Task.CompletedTask;
            return 0;
        }
    }
}
