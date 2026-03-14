using System;
using System.Collections.Generic;
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
    public class CurrenciesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ICbrExchangeRateService _cbrService;

        public CurrenciesController(ApplicationDbContext db, ICbrExchangeRateService cbrService)
        {
            _db = db;
            _cbrService = cbrService;
        }

        [Breadcrumb("Валюты и курсы", FromAction = "Index", FromController = typeof(DashboardController), AreaName = "Finances")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var currencies = await _db.Currencies.Where(c => c.UserUid == userId).ToListAsync();

            var latestRates = new System.Collections.Generic.List<ExchangeRateModel>();
            foreach (var cur in currencies.Where(c => !c.IsBase))
            {
                var rate = await _db.ExchangeRates
                    .Where(r => r.CurrencyId == cur.Id && r.UserUid == userId)
                    .OrderByDescending(r => r.Date)
                    .FirstOrDefaultAsync();
                if (rate != null)
                    latestRates.Add(rate);
            }

            var vm = new CurrencyListViewModel
            {
                Currencies = currencies,
                LatestRates = latestRates,
                PageViewModel = new PageViewModel(currencies.Count, 1, 100)
            };
            return View(vm);
        }

        [Breadcrumb("Новая валюта", FromAction = "Index")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CurrencyModel currency)
        {
            if (ModelState.IsValid)
            {
                currency.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _db.Currencies.Add(currency);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(currency);
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Id == id && c.UserUid == userId);
            if (currency == null) return NotFound();
            return View(currency);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CurrencyModel currency)
        {
            if (ModelState.IsValid)
            {
                currency.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _db.Currencies.Update(currency);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(currency);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRatesFromCbr()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Dictionary<string, decimal> rates;
            try
            {
                rates = await _cbrService.FetchCurrencyRatesAsync(DateTime.Today);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка загрузки курсов ЦБ: {ex.Message}";
                return RedirectToAction("Index");
            }

            var userCurrencies = await _db.Currencies
                .Where(c => c.UserUid == userId && !c.IsBase)
                .ToListAsync();

            int updated = 0;
            foreach (var cur in userCurrencies)
            {
                if (rates.TryGetValue(cur.Code, out var rate))
                {
                    var existing = await _db.ExchangeRates
                        .FirstOrDefaultAsync(r => r.CurrencyId == cur.Id && r.Date.Date == DateTime.Today && r.UserUid == userId);

                    if (existing != null)
                    {
                        existing.Rate = rate;
                        existing.Source = PriceSource.CbrAuto;
                    }
                    else
                    {
                        _db.ExchangeRates.Add(new ExchangeRateModel
                        {
                            CurrencyId = cur.Id,
                            Date = DateTime.Today,
                            Rate = rate,
                            Source = PriceSource.CbrAuto,
                            UserUid = userId
                        });
                    }
                    updated++;
                }
            }

            await _db.SaveChangesAsync();
            TempData["Message"] = $"Курсы обновлены: {updated} валют";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddManualRate(int currencyId, DateTime date, decimal rate)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.ExchangeRates.Add(new ExchangeRateModel
            {
                CurrencyId = currencyId,
                Date = date,
                Rate = rate,
                Source = PriceSource.Manual,
                UserUid = userId
            });
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Id == id && c.UserUid == userId);
            if (currency == null) return NotFound();
            return View(currency);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Id == Id && c.UserUid == userId);
            if (currency != null)
            {
                _db.Currencies.Remove(currency);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
