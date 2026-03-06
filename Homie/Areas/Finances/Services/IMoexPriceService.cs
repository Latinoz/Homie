using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homie.Areas.Finances.Services
{
    /// <summary>Загрузка цен ценных бумаг с Московской биржи (MOEX ISS)</summary>
    public interface IMoexPriceService
    {
        /// <summary>Получить текущие цены акций по тикерам</summary>
        Task<Dictionary<string, decimal>> FetchSharePricesAsync(IEnumerable<string> tickers);

        /// <summary>Получить текущие цены облигаций по тикерам (в % от номинала)</summary>
        Task<Dictionary<string, decimal>> FetchBondPricesAsync(IEnumerable<string> tickers);
    }
}
