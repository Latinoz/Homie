using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homie.Areas.Finances.Services
{
    public class CryptoPriceDto
    {
        public decimal Usd { get; set; }
        public decimal Rub { get; set; }
    }

    /// <summary>Загрузка цен криптовалют через CoinGecko API</summary>
    public interface ICoinGeckoPriceService
    {
        Task<Dictionary<string, CryptoPriceDto>> FetchPricesAsync(IEnumerable<string> coinGeckoIds);
    }
}
