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
    public class PreciousMetalsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PreciousMetalsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Breadcrumb("Драгметаллы", FromAction = "Index", FromController = typeof(DashboardController))]
        public async Task<IActionResult> Index(MetalType? metal, int page = 1,
            FinanceSortState sortOrder = FinanceSortState.NameAsc)
        {
            int pageSize = 20;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<PreciousMetalModel> query = _db.PreciousMetals
                .Where(pm => pm.UserUid == userId);

            if (metal.HasValue)
                query = query.Where(pm => pm.Metal == metal.Value);

            query = sortOrder switch
            {
                FinanceSortState.NameDesc => query.OrderByDescending(pm => pm.Name),
                FinanceSortState.AmountAsc => query.OrderBy(pm => pm.WeightGrams * pm.Quantity * pm.CurrentPricePerGram),
                FinanceSortState.AmountDesc => query.OrderByDescending(pm => pm.WeightGrams * pm.Quantity * pm.CurrentPricePerGram),
                FinanceSortState.TypeAsc => query.OrderBy(pm => pm.Metal),
                FinanceSortState.TypeDesc => query.OrderByDescending(pm => pm.Metal),
                _ => query.OrderBy(pm => pm.Name)
            };

            var count = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Суммы по металлам
            var allMetals = await _db.PreciousMetals.Where(pm => pm.UserUid == userId).ToListAsync();

            var vm = new PreciousMetalListViewModel
            {
                Metals = items,
                PageViewModel = new PageViewModel(count, page, pageSize),
                CurrentSort = sortOrder,
                MetalFilter = metal,
                TotalGoldRub = allMetals.Where(m => m.Metal == MetalType.Gold).Sum(m => m.TotalValueRub),
                TotalSilverRub = allMetals.Where(m => m.Metal == MetalType.Silver).Sum(m => m.TotalValueRub),
                TotalPlatinumRub = allMetals.Where(m => m.Metal == MetalType.Platinum).Sum(m => m.TotalValueRub),
                TotalPalladiumRub = allMetals.Where(m => m.Metal == MetalType.Palladium).Sum(m => m.TotalValueRub)
            };
            return View(vm);
        }

        [Breadcrumb("Новая позиция", FromAction = "Index")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(PreciousMetalModel metal)
        {
            metal.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.PreciousMetals.Add(metal);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var metal = await _db.PreciousMetals.FirstOrDefaultAsync(pm => pm.Id == id && pm.UserUid == userId);
            if (metal == null) return NotFound();
            return View(metal);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PreciousMetalModel metal, bool manualPriceOverride)
        {
            metal.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (manualPriceOverride)
                metal.LastManualOverrideDate = DateTime.UtcNow;
            _db.PreciousMetals.Update(metal);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var metal = await _db.PreciousMetals.FirstOrDefaultAsync(pm => pm.Id == id && pm.UserUid == userId);
            if (metal == null) return NotFound();
            return View(metal);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var metal = await _db.PreciousMetals.FirstOrDefaultAsync(pm => pm.Id == Id && pm.UserUid == userId);
            if (metal != null)
            {
                _db.PreciousMetals.Remove(metal);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
