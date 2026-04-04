using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homie.Areas.Finances.Services
{
    /// <summary>Загрузка цен ценных бумаг с Московской биржи (MOEX ISS)</summary>
    public interface IMoexPriceService
    {
        /// <summary>Получить текущие цены акций по тикерам (борд TQBR)</summary>
        Task<Dictionary<string, decimal>> FetchSharePricesAsync(IEnumerable<string> tickers);

        /// <summary>Получить текущие цены облигаций в абсолютных единицах валюты (% × номинал / 100). Ищет по бордам TQOB и TQCB.</summary>
        Task<Dictionary<string, decimal>> FetchBondPricesAsync(IEnumerable<string> tickers);

        /// <summary>Получить текущие цены ETF по тикерам (борд TQTF)</summary>
        Task<Dictionary<string, decimal>> FetchEtfPricesAsync(IEnumerable<string> tickers);
    }
}
