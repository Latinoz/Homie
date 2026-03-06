using System.Collections.Generic;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Данные для дашборда финансового состояния</summary>
    public class FinanceDashboardViewModel
    {
        /// <summary>Общий капитал в RUB</summary>
        public decimal TotalCapital { get; set; }

        /// <summary>Итого депозиты (RUB)</summary>
        public decimal TotalDeposits { get; set; }

        /// <summary>Итого инвестиции (RUB)</summary>
        public decimal TotalInvestments { get; set; }

        /// <summary>Итого криптовалюта (RUB)</summary>
        public decimal TotalCrypto { get; set; }

        /// <summary>Итого драгметаллы (RUB)</summary>
        public decimal TotalPreciousMetals { get; set; }

        /// <summary>Общая доходность, %</summary>
        public decimal TotalReturnPercent { get; set; }

        /// <summary>Реальная стоимость с учётом инфляции</summary>
        public decimal RealValueAfterInflation { get; set; }

        /// <summary>Накопленная инфляция, %</summary>
        public decimal AccumulatedInflationPercent { get; set; }

        /// <summary>Распределение по классам (JSON для Chart.js pie)</summary>
        public string AssetAllocationJson { get; set; }

        /// <summary>Динамика капитала по месяцам (JSON для Chart.js line)</summary>
        public string CapitalDynamicsJson { get; set; }

        /// <summary>Последняя дата обновления цен</summary>
        public System.DateTime? LastPriceUpdateDate { get; set; }

        /// <summary>Список курсов валют для виджета</summary>
        public List<ExchangeRateModel> LatestRates { get; set; } = new();

        /// <summary>Последние операции для виджета</summary>
        public List<OperationModel> RecentOperations { get; set; } = new();
    }
}
