using System.Collections.Generic;
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

        /// <summary>Создать финансовые счета для криптокошельков, у которых их ещё нет</summary>
        Task<int> EnsureCryptoWalletAccountsAsync(string userId);

        /// <summary>Остатки криптовалюты по криптокошелькам</summary>
        Task<List<WalletCryptoHolding>> GetWalletCryptoHoldingsAsync(string userId);

        /// <summary>Применить операцию покупки/продажи к позиции (инкрементально)</summary>
        Task ApplyOperationToPositionAsync(OperationModel operation, string userId);

        /// <summary>Откатить операцию покупки/продажи из позиции (инкрементально)</summary>
        Task RevertOperationFromPositionAsync(OperationModel operation, string userId);

        /// <summary>Полная синхронизация всех позиций из журнала операций</summary>
        Task<int> SyncAllPositionsFromJournalAsync(string userId);

        /// <summary>Применить операцию покупки/продажи криптовалюты к криптоактиву</summary>
        Task ApplyCryptoOperationAsync(OperationModel operation, string userId);

        /// <summary>Откатить операцию покупки/продажи криптовалюты из криптоактива</summary>
        Task RevertCryptoOperationAsync(OperationModel operation, string userId);

        /// <summary>Полная синхронизация криптоактивов из журнала операций</summary>
        Task<int> SyncCryptoAssetsFromJournalAsync(string userId);

        /// <summary>Синхронизация позиций драгметаллов из журнала операций</summary>
        Task<int> SyncPreciousMetalPositionsFromJournalAsync(string userId);

        /// <summary>Применить операцию покупки/продажи драгметалла к позиции</summary>
        Task ApplyPreciousMetalOperationAsync(OperationModel operation, string userId);

        /// <summary>Откатить операцию покупки/продажи драгметалла из позиции</summary>
        Task RevertPreciousMetalOperationAsync(OperationModel operation, string userId);
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

    /// <summary>Остаток криптовалюты в криптокошельке</summary>
    public class WalletCryptoHolding
    {
        public int WalletId { get; set; }
        public string Ticker { get; set; }
        public string InstrumentName { get; set; }
        public decimal Quantity { get; set; }
        public decimal ValueInRub { get; set; }
    }
}
