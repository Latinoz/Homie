using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Homie.Areas.Finances.Services
{
    /// <summary>Загрузка курсов валют и цен металлов с ЦБ РФ</summary>
    public interface ICbrExchangeRateService
    {
        /// <summary>Получить курсы валют на дату</summary>
        Task<Dictionary<string, decimal>> FetchCurrencyRatesAsync(DateTime date);

        /// <summary>Получить учётные цены драгметаллов ЦБ на дату (RUB/грамм)</summary>
        Task<Dictionary<string, decimal>> FetchMetalPricesAsync(DateTime date);
    }
}
