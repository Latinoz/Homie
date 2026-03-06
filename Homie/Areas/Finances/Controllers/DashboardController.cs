using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Homie.Areas.Finances.Models;
using Homie.Areas.Finances.Services;
using SmartBreadcrumbs.Attributes;

namespace Homie.Areas.Finances.Controllers
{
    [Area("Finances")]
    [Authorize(Roles = "admin,user,finances")]
    public class DashboardController : Controller
    {
        private readonly IFinanceCalculationService _calcService;
        private readonly IPriceUpdateOrchestrator _priceOrchestrator;

        public DashboardController(IFinanceCalculationService calcService, IPriceUpdateOrchestrator priceOrchestrator)
        {
            _calcService = calcService;
            _priceOrchestrator = priceOrchestrator;
        }

        [DefaultBreadcrumb("Финансы")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vm = await _calcService.GetDashboardDataAsync(userId);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAllPrices()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _priceOrchestrator.UpdateAllPricesAsync(userId);
            TempData["PriceUpdateResult"] = $"Обновлено: {result.SuccessCount}, Ошибок: {result.ErrorCount}";
            return RedirectToAction("Index");
        }
    }
}
