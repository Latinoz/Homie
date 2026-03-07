using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Homie.Areas.Finances.Models;
using Homie.Data.Models;
using SmartBreadcrumbs.Attributes;

namespace Homie.Areas.Finances.Controllers
{
    [Area("Finances")]
    [Authorize(Roles = "admin,user,finances")]
    public class OperationTypesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public OperationTypesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Breadcrumb("Типы операций", FromAction = "Index", FromController = typeof(DashboardController), AreaName = "Finances")]
        public async Task<IActionResult> Index(OperationCategory? category)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<OperationTypeModel> query = _db.OperationTypes
                .Where(t => t.UserUid == userId);

            if (category.HasValue)
                query = query.Where(t => t.Category == category.Value);

            var items = await query.OrderBy(t => t.Category).ThenBy(t => t.Name).ToListAsync();

            ViewBag.CategoryFilter = category;
            return View(items);
        }

        [Breadcrumb("Новый тип операции", FromAction = "Index")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(OperationTypeModel operationType)
        {
            operationType.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.OperationTypes.Add(operationType);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var operationType = await _db.OperationTypes.FirstOrDefaultAsync(t => t.Id == id && t.UserUid == userId);
            if (operationType == null) return NotFound();
            return View(operationType);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(OperationTypeModel operationType)
        {
            operationType.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.OperationTypes.Update(operationType);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var operationType = await _db.OperationTypes.FirstOrDefaultAsync(t => t.Id == id && t.UserUid == userId);
            if (operationType == null) return NotFound();

            var isUsed = await _db.FinanceOperations.AnyAsync(o => o.OperationTypeId == id && o.UserUid == userId);
            ViewBag.IsUsed = isUsed;

            return View(operationType);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var isUsed = await _db.FinanceOperations.AnyAsync(o => o.OperationTypeId == Id && o.UserUid == userId);
            if (isUsed)
            {
                TempData["Error"] = "Невозможно удалить тип операции, который используется в журнале операций.";
                return RedirectToAction("Index");
            }

            var operationType = await _db.OperationTypes.FirstOrDefaultAsync(t => t.Id == Id && t.UserUid == userId);
            if (operationType != null)
            {
                _db.OperationTypes.Remove(operationType);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
