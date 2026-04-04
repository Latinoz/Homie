using System.Threading.Tasks;
using Homie.Areas.Finances.Models;

namespace Homie.Areas.Finances.Services
{
    /// <summary>Сервис расчёта финансовых показателей</summary>
    public interface IFinanceCalculationService
    {
        Task<FinanceDashboardViewModel> GetDashboardDataAsync(string userId);
        Task<decimal> GetDepositBalanceAsync(int depositId, string userId);
        Task<InvestmentMetrics> GetInvestmentMetricsAsync(int positionId, string userId);
        Task<CryptoMetrics> GetCryptoMetricsAsync(int assetId, string userId);
        Task<string> GetPortfolioHistoryJsonAsync(string userId, int months = 12);
        Task<string> GetInstrumentPriceChartJsonAsync(int instrumentId, string userId, int months = 12);
    }

    public class InvestmentMetrics
    {
        public decimal QuantityFromJournal { get; set; }
        public decimal AvgPriceFromJournal { get; set; }
        public decimal DividendsFromJournal { get; set; }
        public decimal ReturnPercent { get; set; }
        public decimal ValueInRub { get; set; }
    }

    public class CryptoMetrics
    {
        public decimal QuantityFromJournal { get; set; }
        public decimal ValueInUsd { get; set; }
        public decimal ValueInRub { get; set; }
    }
}
