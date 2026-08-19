using System.ComponentModel.DataAnnotations;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Тип счёта</summary>
    public enum AccountType
    {
        [Display(Name = "Банк")]
        Bank = 0,
        [Display(Name = "Брокер")]
        Broker = 1,
        [Display(Name = "Криптокошелёк")]
        Wallet = 2,
        [Display(Name = "Криптобиржа")]
        CryptoExchange = 3
    }

    /// <summary>Тип кошелька в справочнике</summary>
    public enum WalletType
    {
        [Display(Name = "Обычный")]
        Fiat = 0,
        [Display(Name = "Криптокошелёк")]
        Crypto = 1
    }

    /// <summary>Биржа / источник цен</summary>
    public enum Exchange
    {
        None = 0,
        MOEX = 1,
        NYSE = 2,
        NASDAQ = 3,
        LSE = 4,
        CryptoSpot = 5
    }

    /// <summary>Источник данных о цене / курсе</summary>
    public enum PriceSource
    {
        Manual = 0,
        CbrAuto = 1,
        MoexAuto = 2,
        YahooAuto = 3,
        CoinGeckoAuto = 4
    }

    /// <summary>Категория типа операции</summary>
    public enum OperationCategory
    {
        [Display(Name = "Вклад")]
        Deposit = 0,
        [Display(Name = "Инвестиции")]
        Investment = 1,
        [Display(Name = "Криптовалюта")]
        Crypto = 2,
        [Display(Name = "Драгоценные металлы")]
        PreciousMetal = 3,
        [Display(Name = "Перевод")]
        Transfer = 4,
        [Display(Name = "Другое")]
        Other = 5
    }

    /// <summary>Вид драгметалла</summary>
    public enum MetalType
    {
        [Display(Name = "Золото")]
        Gold = 0,
        [Display(Name = "Серебро")]
        Silver = 1,
        [Display(Name = "Платина")]
        Platinum = 2,
        [Display(Name = "Палладий")]
        Palladium = 3
    }

    /// <summary>Состояния сортировки для модуля Финансы</summary>
    public enum FinanceSortState
    {
        NameAsc,
        NameDesc,
        DateAsc,
        DateDesc,
        AmountAsc,
        AmountDesc,
        CurrencyAsc,
        CurrencyDesc,
        TypeAsc,
        TypeDesc
    }
}
