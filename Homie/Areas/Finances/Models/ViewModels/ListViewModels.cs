using System.Collections.Generic;
using Homie.Models;

namespace Homie.Areas.Finances.Models
{
    /// <summary>ViewModel для списка счетов</summary>
    public class AccountListViewModel
    {
        public IEnumerable<AccountModel> Accounts { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FinanceSortState CurrentSort { get; set; }
        public string NameFilter { get; set; }
        public AccountType? TypeFilter { get; set; }
    }

    /// <summary>ViewModel для списка депозитов</summary>
    public class DepositListViewModel
    {
        public IEnumerable<DepositModel> Deposits { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FinanceSortState CurrentSort { get; set; }
        public string NameFilter { get; set; }
        public int? AccountFilter { get; set; }
    }

    /// <summary>ViewModel для списка инвестиций</summary>
    public class InvestmentListViewModel
    {
        public IEnumerable<InvestmentPositionModel> Positions { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FinanceSortState CurrentSort { get; set; }
        public string NameFilter { get; set; }
        public InstrumentType? TypeFilter { get; set; }
    }

    /// <summary>ViewModel для списка криптоактивов</summary>
    public class CryptoListViewModel
    {
        public IEnumerable<CryptoAssetModel> Assets { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FinanceSortState CurrentSort { get; set; }
        public string NameFilter { get; set; }
    }

    /// <summary>ViewModel для списка драгметаллов</summary>
    public class PreciousMetalListViewModel
    {
        public IEnumerable<PreciousMetalModel> Metals { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FinanceSortState CurrentSort { get; set; }
        public MetalType? MetalFilter { get; set; }
        public decimal TotalGoldRub { get; set; }
        public decimal TotalSilverRub { get; set; }
        public decimal TotalPlatinumRub { get; set; }
        public decimal TotalPalladiumRub { get; set; }
    }

    /// <summary>ViewModel для списка операций</summary>
    public class OperationListViewModel
    {
        public IEnumerable<OperationModel> Operations { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FinanceSortState CurrentSort { get; set; }
        public OperationCategory? CategoryFilter { get; set; }
        public int? AccountFilter { get; set; }
        public System.DateTime? DateFrom { get; set; }
        public System.DateTime? DateTo { get; set; }
    }

    /// <summary>ViewModel для списка инструментов</summary>
    public class InstrumentListViewModel
    {
        public IEnumerable<InstrumentModel> Instruments { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FinanceSortState CurrentSort { get; set; }
        public string NameFilter { get; set; }
        public InstrumentType? TypeFilter { get; set; }
    }

    /// <summary>ViewModel для списка курсов валют</summary>
    public class CurrencyListViewModel
    {
        public IEnumerable<CurrencyModel> Currencies { get; set; }
        public IEnumerable<ExchangeRateModel> LatestRates { get; set; }
        public PageViewModel PageViewModel { get; set; }
    }

    /// <summary>ViewModel для списка инфляции</summary>
    public class InflationListViewModel
    {
        public IEnumerable<InflationModel> Records { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public int? YearFilter { get; set; }
    }
}
