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
using SmartBreadcrumbs.Attributes;

namespace Homie.Areas.Finances.Controllers
{
    [Area("Finances")]
    [Authorize(Roles = "admin,user,finances")]
    public class PriceUpdateController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPriceUpdateOrchestrator _orchestrator;

        public PriceUpdateController(ApplicationDbContext db, IPriceUpdateOrchestrator orchestrator)
        {
            _db = db;
            _orchestrator = orchestrator;
        }

        [Breadcrumb("Обновление цен", FromAction = "Index", FromController = typeof(DashboardController), AreaName = "Finances")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _orchestrator.UpdateAllPricesAsync(userId);
            return View("UpdateResult", result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSingle(int instrumentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _orchestrator.UpdateSinglePriceAsync(instrumentId, userId);
            TempData["PriceUpdateMsg"] = result.Success
                ? $"{result.Ticker}: {result.OldPrice} → {result.NewPrice}"
                : $"{result.Ticker}: {result.ErrorMessage}";
            return RedirectToAction("Index");
        }

        [Breadcrumb("Ручной ввод цены", FromAction = "Index")]
        public IActionResult ManualEntry()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Instruments = _db.Instruments.Where(i => i.UserUid == userId).ToList();
            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            return View(new ManualPriceEntryViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ManualEntry(ManualPriceEntryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var userId2 = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.Instruments = _db.Instruments.Where(i => i.UserUid == userId2).ToList();
                ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId2).ToList();
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Записываем в PriceHistory
            var existingHistory = await _db.PriceHistory
                .FirstOrDefaultAsync(ph => ph.InstrumentId == model.InstrumentId
                                            && ph.Date.Date == model.Date.Date
                                            && ph.UserUid == userId);

            if (existingHistory != null)
            {
                existingHistory.Price = model.Price;
                existingHistory.Source = PriceSource.Manual;
            }
            else
            {
                _db.PriceHistory.Add(new PriceHistoryModel
                {
                    InstrumentId = model.InstrumentId,
                    Date = model.Date,
                    Price = model.Price,
                    CurrencyId = model.CurrencyId,
                    PriceInRub = 0,
                    Source = PriceSource.Manual,
                    UserUid = userId
                });
            }

            // Обновляем текущую цену на инструменте
            if (model.UpdateCurrentPrice)
            {
                var instrument = await _db.Instruments
                    .FirstOrDefaultAsync(i => i.Id == model.InstrumentId && i.UserUid == userId);
                if (instrument != null)
                {
                    instrument.LastPrice = model.Price;
                    instrument.LastPriceDate = model.Date;
                    instrument.LastPriceSource = PriceSource.Manual;
                }

                // Обновляем CurrentPrice на связанных позициях
                var investPos = await _db.InvestmentPositions
                    .Where(ip => ip.InstrumentId == model.InstrumentId && ip.UserUid == userId)
                    .ToListAsync();
                foreach (var pos in investPos)
                {
                    pos.CurrentPrice = model.Price;
                    pos.LastManualOverrideDate = DateTime.UtcNow;
                }

                var cryptoAssets = await _db.CryptoAssets
                    .Where(ca => ca.InstrumentId == model.InstrumentId && ca.UserUid == userId)
                    .ToListAsync();
                foreach (var ca in cryptoAssets)
                {
                    ca.CurrentPrice = model.Price;
                    ca.LastManualOverrideDate = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync();
            TempData["Message"] = $"Цена сохранена: {model.Price} на {model.Date:dd.MM.yyyy}";
            return RedirectToAction("ManualEntry");
        }
    }
}
