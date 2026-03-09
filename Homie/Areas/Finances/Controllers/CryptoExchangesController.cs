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
    public class CryptoExchangesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CryptoExchangesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Breadcrumb("Крипто биржи", FromAction = "Index", FromController = typeof(DashboardController), AreaName = "Finances")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var exchanges = await _db.CryptoExchanges
                .Where(c => c.UserUid == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var vm = new CryptoExchangeListViewModel
            {
                CryptoExchanges = exchanges,
                PageViewModel = new PageViewModel(exchanges.Count, 1, 100)
            };
            return View(vm);
        }

        [Breadcrumb("Новая крипто биржа", FromAction = "Index")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CryptoExchangeModel cryptoExchange)
        {
            cryptoExchange.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.CryptoExchanges.Add(cryptoExchange);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cryptoExchange = await _db.CryptoExchanges.FirstOrDefaultAsync(c => c.Id == id && c.UserUid == userId);
            if (cryptoExchange == null) return NotFound();
            return View(cryptoExchange);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CryptoExchangeModel cryptoExchange)
        {
            cryptoExchange.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.CryptoExchanges.Update(cryptoExchange);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cryptoExchange = await _db.CryptoExchanges.FirstOrDefaultAsync(c => c.Id == id && c.UserUid == userId);
            if (cryptoExchange == null) return NotFound();
            return View(cryptoExchange);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cryptoExchange = await _db.CryptoExchanges.FirstOrDefaultAsync(c => c.Id == Id && c.UserUid == userId);
            if (cryptoExchange != null)
            {
                _db.CryptoExchanges.Remove(cryptoExchange);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
