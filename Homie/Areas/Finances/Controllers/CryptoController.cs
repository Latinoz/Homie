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
                .Include(ca => ca.Account);

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
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Instruments = _db.Instruments
                .Where(i => i.UserUid == userId && i.Type == InstrumentType.Crypto).ToList();
            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            ViewBag.Accounts = _db.FinanceAccounts
                .Where(a => a.UserUid == userId && a.AccountType == AccountType.CryptoExchange).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CryptoAssetModel asset)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid)
            {
                asset.UserUid = userId;
                _db.CryptoAssets.Add(asset);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.Instruments = _db.Instruments
                .Where(i => i.UserUid == userId && i.Type == InstrumentType.Crypto).ToList();
            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            ViewBag.Accounts = _db.FinanceAccounts
                .Where(a => a.UserUid == userId && a.AccountType == AccountType.CryptoExchange).ToList();
            return View(asset);
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

            ViewBag.Instruments = _db.Instruments
                .Where(i => i.UserUid == userId && i.Type == InstrumentType.Crypto).ToList();
            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            ViewBag.Accounts = _db.FinanceAccounts
                .Where(a => a.UserUid == userId && a.AccountType == AccountType.CryptoExchange).ToList();
            return View(asset);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CryptoAssetModel asset, bool manualPriceOverride)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid)
            {
                asset.UserUid = userId;
                if (manualPriceOverride)
                    asset.LastManualOverrideDate = DateTime.UtcNow;
                _db.CryptoAssets.Update(asset);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.Instruments = _db.Instruments
                .Where(i => i.UserUid == userId && i.Type == InstrumentType.Crypto).ToList();
            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            ViewBag.Accounts = _db.FinanceAccounts
                .Where(a => a.UserUid == userId && a.AccountType == AccountType.CryptoExchange).ToList();
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
                .FirstOrDefaultAsync(ca => ca.Id == id && ca.UserUid == userId);
            if (asset == null) return NotFound();

            ViewBag.PriceChartJson = await _calcService.GetInstrumentPriceChartJsonAsync(
                asset.InstrumentId, userId);
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
                _db.CryptoAssets.Remove(asset);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
