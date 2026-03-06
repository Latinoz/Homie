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
    public class InstrumentsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public InstrumentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Breadcrumb("Инструменты", FromAction = "Index", FromController = typeof(DashboardController))]
        public async Task<IActionResult> Index(string name, InstrumentType? type, int page = 1,
            FinanceSortState sortOrder = FinanceSortState.NameAsc)
        {
            int pageSize = 20;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<InstrumentModel> query = _db.Instruments
                .Where(i => i.UserUid == userId)
                .Include(i => i.Currency);

            if (!string.IsNullOrEmpty(name))
                query = query.Where(i => i.Name.Contains(name) || i.Code.Contains(name));
            if (type.HasValue)
                query = query.Where(i => i.Type == type.Value);

            query = sortOrder switch
            {
                FinanceSortState.NameDesc => query.OrderByDescending(i => i.Name),
                FinanceSortState.TypeAsc => query.OrderBy(i => i.Type),
                FinanceSortState.TypeDesc => query.OrderByDescending(i => i.Type),
                _ => query.OrderBy(i => i.Name)
            };

            var count = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new InstrumentListViewModel
            {
                Instruments = items,
                PageViewModel = new PageViewModel(count, page, pageSize),
                CurrentSort = sortOrder,
                NameFilter = name,
                TypeFilter = type
            };
            return View(vm);
        }

        [Breadcrumb("Новый инструмент", FromAction = "Index")]
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(InstrumentModel instrument)
        {
            instrument.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.Instruments.Add(instrument);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var instrument = await _db.Instruments.FirstOrDefaultAsync(i => i.Id == id && i.UserUid == userId);
            if (instrument == null) return NotFound();

            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            return View(instrument);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(InstrumentModel instrument)
        {
            instrument.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.Instruments.Update(instrument);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var instrument = await _db.Instruments
                .Include(i => i.Currency)
                .FirstOrDefaultAsync(i => i.Id == id && i.UserUid == userId);
            if (instrument == null) return NotFound();
            return View(instrument);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var instrument = await _db.Instruments.FirstOrDefaultAsync(i => i.Id == Id && i.UserUid == userId);
            if (instrument != null)
            {
                _db.Instruments.Remove(instrument);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
