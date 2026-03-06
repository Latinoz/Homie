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
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Breadcrumb("Счета", FromAction = "Index", FromController = typeof(DashboardController))]
        public async Task<IActionResult> Index(string name, AccountType? type, int page = 1,
            FinanceSortState sortOrder = FinanceSortState.NameAsc)
        {
            int pageSize = 20;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<AccountModel> query = _db.FinanceAccounts
                .Where(a => a.UserUid == userId)
                .Include(a => a.Currency);

            if (!string.IsNullOrEmpty(name))
                query = query.Where(a => a.Name.Contains(name));
            if (type.HasValue)
                query = query.Where(a => a.AccountType == type.Value);

            query = sortOrder switch
            {
                FinanceSortState.NameDesc => query.OrderByDescending(a => a.Name),
                FinanceSortState.TypeAsc => query.OrderBy(a => a.AccountType),
                FinanceSortState.TypeDesc => query.OrderByDescending(a => a.AccountType),
                _ => query.OrderBy(a => a.Name)
            };

            var count = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new AccountListViewModel
            {
                Accounts = items,
                PageViewModel = new PageViewModel(count, page, pageSize),
                CurrentSort = sortOrder,
                NameFilter = name,
                TypeFilter = type
            };

            return View(vm);
        }

        [Breadcrumb("Новый счёт", FromAction = "Index")]
        public IActionResult Create()
        {
            ViewBag.Currencies = _db.Currencies
                .Where(c => c.UserUid == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AccountModel account)
        {
            account.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.FinanceAccounts.Add(account);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var account = await _db.FinanceAccounts.FirstOrDefaultAsync(a => a.Id == id && a.UserUid == userId);
            if (account == null) return NotFound();

            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            return View(account);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AccountModel account)
        {
            account.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.FinanceAccounts.Update(account);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var account = await _db.FinanceAccounts
                .Include(a => a.Currency)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserUid == userId);
            if (account == null) return NotFound();
            return View(account);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var account = await _db.FinanceAccounts.FirstOrDefaultAsync(a => a.Id == Id && a.UserUid == userId);
            if (account != null)
            {
                _db.FinanceAccounts.Remove(account);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
