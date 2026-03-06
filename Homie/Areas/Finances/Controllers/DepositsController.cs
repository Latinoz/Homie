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
    public class DepositsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFinanceCalculationService _calcService;

        public DepositsController(ApplicationDbContext db, IFinanceCalculationService calcService)
        {
            _db = db;
            _calcService = calcService;
        }

        [Breadcrumb("Депозиты", FromAction = "Index", FromController = typeof(DashboardController))]
        public async Task<IActionResult> Index(string name, int? account, int page = 1,
            FinanceSortState sortOrder = FinanceSortState.NameAsc)
        {
            int pageSize = 20;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<DepositModel> query = _db.Deposits
                .Where(d => d.UserUid == userId)
                .Include(d => d.Account)
                .Include(d => d.Currency);

            if (!string.IsNullOrEmpty(name))
                query = query.Where(d => d.Name.Contains(name));
            if (account.HasValue)
                query = query.Where(d => d.AccountId == account.Value);

            query = sortOrder switch
            {
                FinanceSortState.NameDesc => query.OrderByDescending(d => d.Name),
                FinanceSortState.AmountAsc => query.OrderBy(d => d.Amount),
                FinanceSortState.AmountDesc => query.OrderByDescending(d => d.Amount),
                FinanceSortState.DateAsc => query.OrderBy(d => d.OpenDate),
                FinanceSortState.DateDesc => query.OrderByDescending(d => d.OpenDate),
                _ => query.OrderBy(d => d.Name)
            };

            var count = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new DepositListViewModel
            {
                Deposits = items,
                PageViewModel = new PageViewModel(count, page, pageSize),
                CurrentSort = sortOrder,
                NameFilter = name,
                AccountFilter = account
            };
            return View(vm);
        }

        [Breadcrumb("Новый депозит", FromAction = "Index")]
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Accounts = _db.FinanceAccounts.Where(a => a.UserUid == userId).ToList();
            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(DepositModel deposit)
        {
            deposit.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.Deposits.Add(deposit);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deposit = await _db.Deposits.FirstOrDefaultAsync(d => d.Id == id && d.UserUid == userId);
            if (deposit == null) return NotFound();

            ViewBag.Accounts = _db.FinanceAccounts.Where(a => a.UserUid == userId).ToList();
            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            return View(deposit);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DepositModel deposit)
        {
            deposit.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.Deposits.Update(deposit);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Breadcrumb("Детали", FromAction = "Index")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deposit = await _db.Deposits
                .Include(d => d.Account)
                .Include(d => d.Currency)
                .FirstOrDefaultAsync(d => d.Id == id && d.UserUid == userId);
            if (deposit == null) return NotFound();

            deposit.BalanceFromJournal = await _calcService.GetDepositBalanceAsync(id.Value, userId);
            return View(deposit);
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deposit = await _db.Deposits
                .Include(d => d.Account).Include(d => d.Currency)
                .FirstOrDefaultAsync(d => d.Id == id && d.UserUid == userId);
            if (deposit == null) return NotFound();
            return View(deposit);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deposit = await _db.Deposits.FirstOrDefaultAsync(d => d.Id == Id && d.UserUid == userId);
            if (deposit != null)
            {
                _db.Deposits.Remove(deposit);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
