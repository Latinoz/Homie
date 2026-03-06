using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homie.Areas.Finances.Services
{
    /// <summary>Загрузка цен иностранных ценных бумаг через Yahoo Finance</summary>
    public interface IYahooFinancePriceService
    {
        Task<Dictionary<string, decimal>> FetchPricesAsync(IEnumerable<string> tickers);
    }
}
