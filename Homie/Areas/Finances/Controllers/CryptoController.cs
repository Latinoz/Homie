using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Homie.Areas.Finances.Models;
using Homie.Areas.Finances.Services;
using Homie.Data.Models;
using Homie.Models;
using SmartBreadcrumbs.Attributes;

namespace Homie.Areas.Finances.Controllers
{
    [Area("Finances")]
    [Authorize(Roles = "admin,user,finances")]
    public class CryptoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPriceUpdateOrchestrator _priceOrchestrator;
        private readonly IFinanceCalculationService _calcService;

        public CryptoController(ApplicationDbContext db, IPriceUpdateOrchestrator priceOrchestrator,
            IFinanceCalculationService calcService)
        {
            _db = db;
            _priceOrchestrator = priceOrchestrator;
            _calcService = calcService;
        }

        private void PopulateCryptoViewBag(string userId)
        {
            ViewBag.Instruments = _db.Instruments
                .Where(i => i.UserUid == userId && i.InvestmentType.SystemCode == "Crypto")
                .OrderBy(i => i.Name)
                .ToList();
            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            ViewBag.Accounts = _db.FinanceAccounts
                .Where(a => a.UserUid == userId
                         && (a.AccountType == AccountType.CryptoExchange
                             || (a.AccountType == AccountType.Wallet
                                 && (a.WalletId == null || a.Wallet.Type == WalletType.Crypto))))
                .Include(a => a.Wallet)
                .Include(a => a.CryptoExchange)
                .OrderBy(a => a.AccountType).ThenBy(a => a.Name)
                .ToList();
            ViewBag.HasCryptoWallets = _db.Wallets
                .Any(w => w.UserUid == userId && w.Type == WalletType.Crypto);
            ViewBag.HasCryptoInstruments = _db.Instruments
                .Any(i => i.UserUid == userId && i.InvestmentType.SystemCode == "Crypto");
        }

        private async Task PopulateUnlinkedCryptoWalletsAsync(string userId)
        {
            var cryptoWalletIds = await _db.Wallets
                .Where(w => w.UserUid == userId && w.Type == WalletType.Crypto)
                .Select(w => w.Id)
                .ToListAsync();
            var linkedWalletIds = await _db.FinanceAccounts
                .Where(a => a.UserUid == userId && a.AccountType == AccountType.Wallet && a.WalletId != null)
                .Select(a => a.WalletId.Value)
                .ToListAsync();
            var unlinkedIds = cryptoWalletIds.Where(id => !linkedWalletIds.Contains(id)).ToList();

            ViewBag.UnlinkedCryptoWallets = unlinkedIds.Any()
                ? await _db.Wallets.Where(w => unlinkedIds.Contains(w.Id)).Select(w => w.Name).ToListAsync()
                : new System.Collections.Generic.List<string>();
        }

        [Breadcrumb("Криптовалюта", FromAction = "Index", FromController = typeof(DashboardController), AreaName = "Finances")]
        public async Task<IActionResult> Index(string name, int page = 1,
            FinanceSortState sortOrder = FinanceSortState.NameAsc)
        {
            int pageSize = 20;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<CryptoAssetModel> query = _db.CryptoAssets
                .Where(ca => ca.UserUid == userId)
                .Include(ca => ca.Instrument)
                .Include(ca => ca.Currency)
                .Include(ca => ca.Account)
                    .ThenInclude(a => a.Wallet)
                .Include(ca => ca.Account)
                    .ThenInclude(a => a.CryptoExchange);

            if (!string.IsNullOrEmpty(name))
                query = query.Where(ca => ca.Ticker.Contains(name) || ca.Instrument.Name.Contains(name));

            query = sortOrder switch
            {
                FinanceSortState.NameDesc => query.OrderByDescending(ca => ca.Ticker),
                FinanceSortState.AmountAsc => query.OrderBy(ca => ca.Quantity * ca.CurrentPrice),
                FinanceSortState.AmountDesc => query.OrderByDescending(ca => ca.Quantity * ca.CurrentPrice),
                _ => query.OrderBy(ca => ca.Ticker)
            };

            var count = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new CryptoListViewModel
            {
                Assets = items,
                PageViewModel = new PageViewModel(count, page, pageSize),
                CurrentSort = sortOrder,
                NameFilter = name
            };
            return View(vm);
        }

        [Breadcrumb("Новый криптоактив", FromAction = "Index")]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Создать связанные счета для криптокошельков, у которых их ещё нет
            await _calcService.EnsureCryptoWalletAccountsAsync(userId);

            PopulateCryptoViewBag(userId);
            await PopulateUnlinkedCryptoWalletsAsync(userId);
            return View(new CryptoAssetModel
            {
                PurchaseDate = DateTime.Today,
                IsAutoUpdateEnabled = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CryptoAssetModel asset)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            asset.UserUid = userId;

            if (asset.AccountId == null || asset.AccountId == 0)
                ModelState.AddModelError("AccountId", "Выберите кошелёк или биржу");
            if (asset.Quantity > 0 && asset.AvgPurchasePrice <= 0)
                ModelState.AddModelError("AvgPurchasePrice", "Укажите среднюю цену покупки для количества больше нуля");

            if (ModelState.IsValid)
            {
                asset.UserUid = userId;
                try
                {
                    _db.CryptoAssets.Add(asset);
                    await _db.SaveChangesAsync();

                    // Количество — из журнала: автоматически создаём операцию покупки
                    if (asset.Quantity > 0)
                    {
                        await CreateBuyOperationAsync(asset, userId);
                    }

                    return RedirectToAction("Index");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("",
                        "Не удалось сохранить: значение вне допустимого диапазона. Проверьте «Количество», «Среднюю цену покупки» и «Текущую цену» — число без пробелов, до 12 знаков после запятой.");
                }
            }

            await _calcService.EnsureCryptoWalletAccountsAsync(userId);
            PopulateCryptoViewBag(userId);
            await PopulateUnlinkedCryptoWalletsAsync(userId);
            return View(asset);
        }

        private async Task CreateBuyOperationAsync(CryptoAssetModel asset, string userId)
        {
            var buyType = await _db.OperationTypes
                .FirstOrDefaultAsync(ot => ot.Name == "Покупка криптовалюты" && ot.UserUid == null);

            decimal rubRate = 1m;
            var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Id == asset.CurrencyId);
            if (currency != null && !currency.IsBase)
            {
                var rate = await _db.ExchangeRates
                    .Where(r => r.CurrencyId == asset.CurrencyId && r.UserUid == userId)
                    .OrderByDescending(r => r.Date)
                    .FirstOrDefaultAsync();
                rubRate = rate?.Rate ?? 1m;
            }

            _db.FinanceOperations.Add(new OperationModel
            {
                Date = asset.PurchaseDate ?? DateTime.Today,
                OperationTypeId = buyType?.Id ?? 10,
                AccountId = asset.AccountId.Value,
                InstrumentId = asset.InstrumentId,
                Quantity = asset.Quantity,
                Price = asset.AvgPurchasePrice,
                CurrencyId = asset.CurrencyId.Value,
                ExchangeRateToRub = rubRate,
                AmountInRub = asset.Quantity * asset.AvgPurchasePrice * rubRate,
                Notes = "Автоматически создано при добавлении криптоактива",
                UserUid = userId
            });
            await _db.SaveChangesAsync();
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var asset = await _db.CryptoAssets
                .Include(ca => ca.Instrument)
                .FirstOrDefaultAsync(ca => ca.Id == id && ca.UserUid == userId);
            if (asset == null) return NotFound();

            await _calcService.EnsureCryptoWalletAccountsAsync(userId);
            PopulateCryptoViewBag(userId);
            return View(asset);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CryptoAssetModel asset, bool manualPriceOverride)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            asset.UserUid = userId;

            var old = await _db.CryptoAssets.AsNoTracking()
                .FirstOrDefaultAsync(ca => ca.Id == asset.Id && ca.UserUid == userId);
            if (old == null) return NotFound();

            if (old.AccountId != asset.AccountId)
            {
                var hasJournalOps = await _db.FinanceOperations
                    .AnyAsync(o => o.UserUid == userId
                                && o.InstrumentId == asset.InstrumentId
                                && o.AccountId == old.AccountId
                                && (o.OperationType.Name == "Покупка криптовалюты"
                                 || o.OperationType.Name == "Продажа криптовалюты"));
                if (hasJournalOps)
                    ModelState.AddModelError("AccountId",
                        "По этому активу есть операции в журнале. Перенос между кошельками/биржами оформляйте через Журнал операций (продажа и покупка).");
            }

            if (ModelState.IsValid)
            {
                asset.UserUid = userId;
                if (manualPriceOverride)
                    asset.LastManualOverrideDate = DateTime.UtcNow;
                try
                {
                    _db.CryptoAssets.Update(asset);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("",
                        "Не удалось сохранить: значение вне допустимого диапазона. Проверьте «Текущую цену» — число без пробелов, до 12 знаков после запятой.");
                }
            }

            await _calcService.EnsureCryptoWalletAccountsAsync(userId);
            PopulateCryptoViewBag(userId);
            return View(asset);
        }

        [Breadcrumb("Детали", FromAction = "Index")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var asset = await _db.CryptoAssets
                .Include(ca => ca.Instrument).ThenInclude(i => i.Currency)
                .Include(ca => ca.Currency)
                .Include(ca => ca.Account)
                    .ThenInclude(a => a.Wallet)
                .Include(ca => ca.Account)
                    .ThenInclude(a => a.CryptoExchange)
                .FirstOrDefaultAsync(ca => ca.Id == id && ca.UserUid == userId);
            if (asset == null) return NotFound();

            var metrics = await _calcService.GetCryptoMetricsAsync(id.Value, userId);
            asset.QuantityFromJournal = metrics.QuantityFromJournal;
            asset.ValueInUsd = metrics.ValueInUsd;
            asset.ValueInRub = metrics.ValueInRub;

            ViewBag.PriceChartJson = await _calcService.GetInstrumentPriceChartJsonAsync(
                asset.InstrumentId.Value, userId);
            return View(asset);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePrice(int instrumentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _priceOrchestrator.UpdateSinglePriceAsync(instrumentId, userId);
            TempData["PriceUpdateMsg"] = result.Success
                ? $"{result.Ticker}: {result.OldPrice} → {result.NewPrice}"
                : $"{result.Ticker}: ошибка - {result.ErrorMessage}";
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var asset = await _db.CryptoAssets
                .Include(ca => ca.Instrument)
                .FirstOrDefaultAsync(ca => ca.Id == id && ca.UserUid == userId);
            if (asset == null) return NotFound();
            return View(asset);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var asset = await _db.CryptoAssets.FirstOrDefaultAsync(ca => ca.Id == Id && ca.UserUid == userId);
            if (asset != null)
            {
                var hasOps = await _db.FinanceOperations
                    .AnyAsync(o => o.UserUid == userId
                                && o.InstrumentId == asset.InstrumentId
                                && o.AccountId == asset.AccountId
                                && (o.OperationType.Name == "Покупка криптовалюты"
                                 || o.OperationType.Name == "Продажа криптовалюты"));
                if (hasOps)
                {
                    TempData["CryptoErrorMsg"] =
                        "Невозможно удалить актив: по нему есть операции в журнале. Сначала удалите их в Журнале операций.";
                    return RedirectToAction("Index");
                }

                _db.CryptoAssets.Remove(asset);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
