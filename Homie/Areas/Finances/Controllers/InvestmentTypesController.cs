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
    public class InvestmentTypesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public InvestmentTypesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Breadcrumb("Типы инструментов", FromAction = "Index", FromController = typeof(DashboardController), AreaName = "Finances")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var types = await _db.InvestmentTypes
                .Where(t => t.UserUid == null || t.UserUid == userId)
                .OrderBy(t => t.Id)
                .ToListAsync();

            var vm = new InvestmentTypeListViewModel
            {
                InvestmentTypes = types,
                PageViewModel = new PageViewModel(types.Count, 1, 100)
            };
            return View(vm);
        }

        [Breadcrumb("Новый тип", FromAction = "Index")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(InvestmentTypeModel model)
        {
            model.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.SystemCode = null; // пользовательские типы без системного кода
            _db.InvestmentTypes.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var type = await _db.InvestmentTypes
                .FirstOrDefaultAsync(t => t.Id == id && (t.UserUid == null || t.UserUid == userId));
            if (type == null) return NotFound();
            return View(type);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(InvestmentTypeModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existing = await _db.InvestmentTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == model.Id && (t.UserUid == null || t.UserUid == userId));
            if (existing == null) return NotFound();

            // Сохраняем оригинальные значения системных полей
            model.SystemCode = existing.SystemCode;
            model.UserUid = existing.UserUid;
            _db.InvestmentTypes.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var type = await _db.InvestmentTypes
                .FirstOrDefaultAsync(t => t.Id == id && (t.UserUid == null || t.UserUid == userId));
            if (type == null) return NotFound();
            return View(type);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var type = await _db.InvestmentTypes
                .FirstOrDefaultAsync(t => t.Id == Id && (t.UserUid == null || t.UserUid == userId));
            if (type == null) return NotFound();

            // Запретить удаление системных типов
            if (type.SystemCode != null)
            {
                TempData["Error"] = "Нельзя удалить системный тип инструмента.";
                return RedirectToAction("Index");
            }

            // Проверить, не используется ли тип
            var inUse = await _db.Instruments.AnyAsync(i => i.InvestmentTypeId == Id);
            if (inUse)
            {
                TempData["Error"] = "Тип используется в инструментах. Сначала измените тип у инструментов.";
                return RedirectToAction("Index");
            }

            _db.InvestmentTypes.Remove(type);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
