namespace Homie.Areas.Finances.Models
{
    /// <summary>Тип счёта</summary>
    public enum AccountType
    {
        Bank = 0,
        Broker = 1,
        Wallet = 2,
        CryptoExchange = 3
    }

    /// <summary>Тип инструмента</summary>
    public enum InstrumentType
    {
        Stock = 0,
        Bond = 1,
        ETF = 2,
        Crypto = 3,
        PreciousMetalRef = 4
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
        Deposit = 0,
        Investment = 1,
        Crypto = 2,
        PreciousMetal = 3,
        Transfer = 4,
        Other = 5
    }

    /// <summary>Вид драгметалла</summary>
    public enum MetalType
    {
        Gold = 0,
        Silver = 1,
        Platinum = 2,
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
