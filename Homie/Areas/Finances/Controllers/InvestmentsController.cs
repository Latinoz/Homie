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
    public class InvestmentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFinanceCalculationService _calcService;
        private readonly IPriceUpdateOrchestrator _priceOrchestrator;

        public InvestmentsController(ApplicationDbContext db, IFinanceCalculationService calcService,
            IPriceUpdateOrchestrator priceOrchestrator)
        {
            _db = db;
            _calcService = calcService;
            _priceOrchestrator = priceOrchestrator;
        }

        [Breadcrumb("Инвестиции", FromAction = "Index", FromController = typeof(DashboardController), AreaName = "Finances")]
        public async Task<IActionResult> Index(string name, InstrumentType? type, int page = 1,
            FinanceSortState sortOrder = FinanceSortState.NameAsc)
        {
            int pageSize = 20;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<InvestmentPositionModel> query = _db.InvestmentPositions
                .Where(ip => ip.UserUid == userId)
                .Include(ip => ip.Instrument).ThenInclude(i => i.Currency)
                .Include(ip => ip.Account);

            if (!string.IsNullOrEmpty(name))
                query = query.Where(ip => ip.Instrument.Name.Contains(name) || ip.Instrument.Code.Contains(name));
            if (type.HasValue)
                query = query.Where(ip => ip.Instrument.Type == type.Value);

            query = sortOrder switch
            {
                FinanceSortState.NameDesc => query.OrderByDescending(ip => ip.Instrument.Name),
                FinanceSortState.AmountAsc => query.OrderBy(ip => ip.Quantity * ip.CurrentPrice),
                FinanceSortState.AmountDesc => query.OrderByDescending(ip => ip.Quantity * ip.CurrentPrice),
                _ => query.OrderBy(ip => ip.Instrument.Name)
            };

            var count = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new InvestmentListViewModel
            {
                Positions = items,
                PageViewModel = new PageViewModel(count, page, pageSize),
                CurrentSort = sortOrder,
                NameFilter = name,
                TypeFilter = type
            };
            return View(vm);
        }

        [Breadcrumb("Новая позиция", FromAction = "Index")]
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Instruments = _db.Instruments
                .Where(i => i.UserUid == userId && (i.Type == InstrumentType.Stock || i.Type == InstrumentType.Bond || i.Type == InstrumentType.ETF))
                .ToList();
            ViewBag.Accounts = _db.FinanceAccounts
                .Where(a => a.UserUid == userId && a.AccountType == AccountType.Broker)
                .ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(InvestmentPositionModel position)
        {
            position.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.InvestmentPositions.Add(position);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var position = await _db.InvestmentPositions
                .Include(ip => ip.Instrument)
                .FirstOrDefaultAsync(ip => ip.Id == id && ip.UserUid == userId);
            if (position == null) return NotFound();

            ViewBag.Instruments = _db.Instruments
                .Where(i => i.UserUid == userId && (i.Type == InstrumentType.Stock || i.Type == InstrumentType.Bond || i.Type == InstrumentType.ETF))
                .ToList();
            ViewBag.Accounts = _db.FinanceAccounts
                .Where(a => a.UserUid == userId && a.AccountType == AccountType.Broker).ToList();
            return View(position);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(InvestmentPositionModel position, bool manualPriceOverride)
        {
            position.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (manualPriceOverride)
            {
                position.LastManualOverrideDate = DateTime.UtcNow;
            }
            _db.InvestmentPositions.Update(position);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Breadcrumb("Детали", FromAction = "Index")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var position = await _db.InvestmentPositions
                .Include(ip => ip.Instrument).ThenInclude(i => i.Currency)
                .Include(ip => ip.Account)
                .FirstOrDefaultAsync(ip => ip.Id == id && ip.UserUid == userId);
            if (position == null) return NotFound();

            var metrics = await _calcService.GetInvestmentMetricsAsync(id.Value, userId);
            position.QuantityFromJournal = metrics.QuantityFromJournal;
            position.AvgPriceFromJournal = metrics.AvgPriceFromJournal;
            position.DividendsFromJournal = metrics.DividendsFromJournal;
            position.ReturnPercent = metrics.ReturnPercent;
            position.ValueInRub = metrics.ValueInRub;

            ViewBag.PriceChartJson = await _calcService.GetInstrumentPriceChartJsonAsync(
                position.InstrumentId, userId);

            return View(position);
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
            var position = await _db.InvestmentPositions
                .Include(ip => ip.Instrument).Include(ip => ip.Account)
                .FirstOrDefaultAsync(ip => ip.Id == id && ip.UserUid == userId);
            if (position == null) return NotFound();
            return View(position);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var pos = await _db.InvestmentPositions.FirstOrDefaultAsync(ip => ip.Id == Id && ip.UserUid == userId);
            if (pos != null)
            {
                _db.InvestmentPositions.Remove(pos);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
