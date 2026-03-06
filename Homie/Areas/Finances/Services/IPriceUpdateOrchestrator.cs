using System.Threading.Tasks;
using Homie.Areas.Finances.Models;

namespace Homie.Areas.Finances.Services
{
    /// <summary>Координатор обновления всех цен</summary>
    public interface IPriceUpdateOrchestrator
    {
        /// <summary>Обновить все цены для пользователя</summary>
        Task<PriceUpdateResultViewModel> UpdateAllPricesAsync(string userId);

        /// <summary>Обновить цену одного инструмента</summary>
        Task<PriceUpdateItemResult> UpdateSinglePriceAsync(int instrumentId, string userId);
    }
}
