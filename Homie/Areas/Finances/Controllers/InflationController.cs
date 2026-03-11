using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Homie.Areas.Finances.Models;
using Homie.Data.Models;
using Homie.Models;
using SmartBreadcrumbs.Attributes;

namespace Homie.Areas.Finances.Controllers
{
    [Area("Finances")]
    [Authorize(Roles = "admin,user,finances")]
    public class InflationController : Controller
    {
        private readonly ApplicationDbContext _db;

        public InflationController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Breadcrumb("Инфляция", FromAction = "Index", FromController = typeof(DashboardController), AreaName = "Finances")]
        public async Task<IActionResult> Index(int? year, int page = 1)
        {
            int pageSize = 24;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<InflationModel> query = _db.Inflation
                .Where(i => i.UserUid == userId);

            if (year.HasValue)
                query = query.Where(i => i.Year == year.Value);

            query = query.OrderByDescending(i => i.Year).ThenByDescending(i => i.Month);

            var count = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new InflationListViewModel
            {
                Records = items,
                PageViewModel = new PageViewModel(count, page, pageSize),
                YearFilter = year
            };
            return View(vm);
        }

        [Breadcrumb("Новая запись", FromAction = "Index")]
        public IActionResult Create()
        {
            return View(new InflationModel { Year = DateTime.Now.Year, Month = DateTime.Now.Month });
        }

        [HttpPost]
        public async Task<IActionResult> Create(InflationModel inflation)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                inflation.UserUid = userId;

                // Автоподсчёт накопленной инфляции за год
                var prevMonth = await _db.Inflation
                    .Where(i => i.UserUid == userId && i.Year == inflation.Year && i.Month == inflation.Month - 1)
                    .FirstOrDefaultAsync();

                if (prevMonth != null && prevMonth.AccumulatedYearPercent.HasValue)
                {
                    inflation.AccumulatedYearPercent =
                        (1 + prevMonth.AccumulatedYearPercent.Value / 100m) * (1 + inflation.CpiPercent / 100m) * 100m - 100m;
                }
                else
                {
                    inflation.AccumulatedYearPercent = inflation.CpiPercent;
                }

                _db.Inflation.Add(inflation);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(inflation);
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var inflation = await _db.Inflation.FirstOrDefaultAsync(i => i.Id == id && i.UserUid == userId);
            if (inflation == null) return NotFound();
            return View(inflation);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(InflationModel inflation)
        {
            if (ModelState.IsValid)
            {
                inflation.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _db.Inflation.Update(inflation);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(inflation);
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var inflation = await _db.Inflation.FirstOrDefaultAsync(i => i.Id == id && i.UserUid == userId);
            if (inflation == null) return NotFound();
            return View(inflation);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var inflation = await _db.Inflation.FirstOrDefaultAsync(i => i.Id == Id && i.UserUid == userId);
            if (inflation != null)
            {
                _db.Inflation.Remove(inflation);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
