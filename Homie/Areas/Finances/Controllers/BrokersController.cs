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
    public class BrokersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BrokersController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Breadcrumb("Брокеры", FromAction = "Index", FromController = typeof(DashboardController), AreaName = "Finances")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var brokers = await _db.Brokers
                .Where(b => b.UserUid == userId)
                .OrderBy(b => b.Name)
                .ToListAsync();

            var vm = new BrokerListViewModel
            {
                Brokers = brokers,
                PageViewModel = new PageViewModel(brokers.Count, 1, 100)
            };
            return View(vm);
        }

        [Breadcrumb("Новый брокер", FromAction = "Index")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(BrokerModel broker)
        {
            if (ModelState.IsValid)
            {
                broker.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _db.Brokers.Add(broker);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(broker);
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var broker = await _db.Brokers.FirstOrDefaultAsync(b => b.Id == id && b.UserUid == userId);
            if (broker == null) return NotFound();
            return View(broker);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BrokerModel broker)
        {
            if (ModelState.IsValid)
            {
                broker.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _db.Brokers.Update(broker);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(broker);
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var broker = await _db.Brokers.FirstOrDefaultAsync(b => b.Id == id && b.UserUid == userId);
            if (broker == null) return NotFound();
            return View(broker);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var broker = await _db.Brokers.FirstOrDefaultAsync(b => b.Id == Id && b.UserUid == userId);
            if (broker != null)
            {
                _db.Brokers.Remove(broker);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
