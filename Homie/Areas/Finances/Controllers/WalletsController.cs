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
    public class WalletsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFinanceCalculationService _calcService;

        public WalletsController(ApplicationDbContext db, IFinanceCalculationService calcService)
        {
            _db = db;
            _calcService = calcService;
        }

        [Breadcrumb("Кошельки", FromAction = "Index", FromController = typeof(DashboardController), AreaName = "Finances")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wallets = await _db.Wallets
                .Where(w => w.UserUid == userId)
                .Include(w => w.Currency)
                .OrderBy(w => w.Name)
                .ToListAsync();

            // Лениво создать связанные счета для криптокошельков
            await _calcService.EnsureCryptoWalletAccountsAsync(userId);

            var holdings = await _calcService.GetWalletCryptoHoldingsAsync(userId);

            // Криптокошельки, для которых счёт так и не создан (нет валют)
            var cryptoWalletIds = wallets.Where(w => w.Type == WalletType.Crypto).Select(w => w.Id).ToList();
            var linkedWalletIds = await _db.FinanceAccounts
                .Where(a => a.UserUid == userId && a.AccountType == AccountType.Wallet && a.WalletId != null)
                .Select(a => a.WalletId.Value)
                .ToListAsync();

            var vm = new WalletListViewModel
            {
                Wallets = wallets,
                CryptoHoldings = holdings,
                CryptoWalletsWithoutAccount = cryptoWalletIds.Where(id => !linkedWalletIds.Contains(id)).ToList(),
                PageViewModel = new PageViewModel(wallets.Count, 1, 100)
            };
            return View(vm);
        }

        [Breadcrumb("Новый кошелёк", FromAction = "Index")]
        public IActionResult Create(string returnUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
            return View(new WalletModel { Type = WalletType.Crypto });
        }

        [HttpPost]
        public async Task<IActionResult> Create(WalletModel wallet, string returnUrl)
        {
            wallet.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.Wallets.Add(wallet);
            await _db.SaveChangesAsync();

            await _calcService.EnsureCryptoWalletAccountsAsync(wallet.UserUid);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            return RedirectToAction("Index");
        }

        [Breadcrumb("Редактирование", FromAction = "Index")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.Id == id && w.UserUid == userId);
            if (wallet == null) return NotFound();
            ViewBag.Currencies = _db.Currencies.Where(c => c.UserUid == userId).ToList();
            return View(wallet);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(WalletModel wallet)
        {
            wallet.UserUid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.Wallets.Update(wallet);
            await _db.SaveChangesAsync();

            await _calcService.EnsureCryptoWalletAccountsAsync(wallet.UserUid);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wallet = await _db.Wallets
                .Include(w => w.Currency)
                .FirstOrDefaultAsync(w => w.Id == id && w.UserUid == userId);
            if (wallet == null) return NotFound();
            return View(wallet);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.Id == Id && w.UserUid == userId);
            if (wallet != null)
            {
                _db.Wallets.Remove(wallet);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
