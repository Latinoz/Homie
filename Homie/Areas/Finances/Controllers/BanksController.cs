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
    public class BanksController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BanksController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Breadcrumb("Банки", FromAction = "Index", FromController = typeof(DashboardController), AreaName = "Finances")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var banks = await _db.Banks
                .Where(b => b.UserUid == userId)
                .OrderBy(b => b.Name)
                .ToListAsync();

            var vm = new BankListViewModel
            {
                Banks = banks,
                PageViewModel = new PageViewModel(banks.Count, 1, 100)
            };
            return View(vm);
        }

        [Breadcrumb("Новый банк", FromAction = "Index")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(BankModel bank)
        {
            bank.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.Banks.Add(bank);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bank = await _db.Banks.FirstOrDefaultAsync(b => b.Id == id && b.UserUid == userId);
            if (bank == null) return NotFound();
            return View(bank);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BankModel bank)
        {
            bank.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.Banks.Update(bank);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bank = await _db.Banks.FirstOrDefaultAsync(b => b.Id == id && b.UserUid == userId);
            if (bank == null) return NotFound();
            return View(bank);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bank = await _db.Banks.FirstOrDefaultAsync(b => b.Id == Id && b.UserUid == userId);
            if (bank != null)
            {
                _db.Banks.Remove(bank);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
