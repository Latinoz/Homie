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
    public class OperationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFinanceCalculationService _calcService;

        public OperationsController(ApplicationDbContext db, IFinanceCalculationService calcService)
        {
            _db = db;
            _calcService = calcService;
        }

        private static readonly string[] SecuritiesOperationNames =
            { "Покупка ценных бумаг", "Продажа ценных бумаг" };

        private async Task<bool> IsSecuritiesOperationAsync(OperationModel op)
        {
            var opType = await _db.OperationTypes.FindAsync(op.OperationTypeId);
            op.OperationType = opType;
            return opType != null && SecuritiesOperationNames.Contains(opType.Name);
        }

        [Breadcrumb("Журнал операций", FromAction = "Index", FromController = typeof(DashboardController), AreaName = "Finances")]
        public async Task<IActionResult> Index(OperationCategory? category, int? account,
            DateTime? dateFrom, DateTime? dateTo, int page = 1,
            FinanceSortState sortOrder = FinanceSortState.DateDesc)
        {
            int pageSize = 30;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<OperationModel> query = _db.FinanceOperations
                .Where(o => o.UserUid == userId)
                .Include(o => o.OperationType)
                .Include(o => o.Account)
                .Include(o => o.Instrument)
                .Include(o => o.Currency);

            if (category.HasValue)
                query = query.Where(o => o.OperationType.Category == category.Value);
            if (account.HasValue)
                query = query.Where(o => o.AccountId == account.Value);
            if (dateFrom.HasValue)
                query = query.Where(o => o.Date >= dateFrom.Value);
            if (dateTo.HasValue)
                query = query.Where(o => o.Date <= dateTo.Value);

            query = sortOrder switch
            {
                FinanceSortState.DateAsc => query.OrderBy(o => o.Date),
                FinanceSortState.AmountAsc => query.OrderBy(o => o.AmountInRub),
                FinanceSortState.AmountDesc => query.OrderByDescending(o => o.AmountInRub),
                FinanceSortState.TypeAsc => query.OrderBy(o => o.OperationType.Name),
                FinanceSortState.TypeDesc => query.OrderByDescending(o => o.OperationType.Name),
                _ => query.OrderByDescending(o => o.Date)
            };

            var count = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new OperationListViewModel
            {
                Operations = items,
                PageViewModel = new PageViewModel(count, page, pageSize),
                CurrentSort = sortOrder,
                CategoryFilter = category,
                AccountFilter = account,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            ViewBag.Accounts = _db.FinanceAccounts.Where(a => a.UserUid == userId).ToList();
            return View(vm);
        }

        [Breadcrumb("Новая операция", FromAction = "Index")]
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.OperationTypes = _db.OperationTypes.ToList();
            ViewBag.Accounts = _db.FinanceAccounts.Where(a => a.UserUid == userId).ToList();
            ViewBag.Instruments = _db.Instruments.Where(i => i.UserUid == userId).ToList();
            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(OperationModel operation)
        {
            operation.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.FinanceOperations.Add(operation);
            await _db.SaveChangesAsync();

            if (operation.InstrumentId.HasValue && await IsSecuritiesOperationAsync(operation))
            {
                await _calcService.ApplyOperationToPositionAsync(operation, operation.UserUid);
            }

            return RedirectToAction("Index");
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var operation = await _db.FinanceOperations
                .FirstOrDefaultAsync(o => o.Id == id && o.UserUid == userId);
            if (operation == null) return NotFound();

            ViewBag.OperationTypes = _db.OperationTypes.ToList();
            ViewBag.Accounts = _db.FinanceAccounts.Where(a => a.UserUid == userId).ToList();
            ViewBag.Instruments = _db.Instruments.Where(i => i.UserUid == userId).ToList();
            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            return View(operation);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(OperationModel operation)
        {
            operation.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Загрузить старую версию операции для отката
            var oldOp = await _db.FinanceOperations.AsNoTracking()
                .Include(o => o.OperationType)
                .FirstOrDefaultAsync(o => o.Id == operation.Id && o.UserUid == operation.UserUid);

            // Откатить старую операцию из позиции
            if (oldOp != null && oldOp.InstrumentId.HasValue
                && SecuritiesOperationNames.Contains(oldOp.OperationType?.Name))
            {
                await _calcService.RevertOperationFromPositionAsync(oldOp, operation.UserUid);
            }

            _db.FinanceOperations.Update(operation);
            await _db.SaveChangesAsync();

            // Применить новую операцию к позиции
            if (operation.InstrumentId.HasValue && await IsSecuritiesOperationAsync(operation))
            {
                await _calcService.ApplyOperationToPositionAsync(operation, operation.UserUid);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var operation = await _db.FinanceOperations
                .Include(o => o.OperationType).Include(o => o.Account)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserUid == userId);
            if (operation == null) return NotFound();
            return View(operation);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var operation = await _db.FinanceOperations
                .Include(o => o.OperationType)
                .FirstOrDefaultAsync(o => o.Id == Id && o.UserUid == userId);
            if (operation != null)
            {
                bool isSecurities = operation.InstrumentId.HasValue
                    && operation.OperationType != null
                    && SecuritiesOperationNames.Contains(operation.OperationType.Name);

                // Откатить операцию из позиции ДО удаления
                if (isSecurities)
                {
                    await _calcService.RevertOperationFromPositionAsync(operation, userId);
                }

                _db.FinanceOperations.Remove(operation);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
