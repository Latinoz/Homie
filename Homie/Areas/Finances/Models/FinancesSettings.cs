namespace Homie.Areas.Finances.Models
{
    /// <summary>POCO для секции Finances в appsettings.json</summary>
    public class FinancesSettings
    {
        public string BaseCurrency { get; set; } = "RUB";
        public string CbrUpdateTimeUtc { get; set; } = "08:30";
        public string PriceUpdateTimeUtc { get; set; } = "19:00";
        public bool EnableBackgroundPriceUpdate { get; set; } = true;
        public int ManualOverrideLockHours { get; set; } = 24;
        public string[] DefaultCurrencies { get; set; } = new[] { "RUB", "USD", "EUR", "CNY", "GBP" };
        public string[] DefaultMetals { get; set; } = new[] { "Gold", "Silver", "Platinum", "Palladium" };
        public CoinGeckoSettings CoinGecko { get; set; } = new();
        public MoexIssSettings MoexIss { get; set; } = new();
        public YahooFinanceSettings YahooFinance { get; set; } = new();
    }

    public class CoinGeckoSettings
    {
        public string BaseUrl { get; set; } = "https://api.coingecko.com/api/v3";
        public int RequestDelayMs { get; set; } = 2100;
    }

    public class MoexIssSettings
    {
        public string BaseUrl { get; set; } = "https://iss.moex.com/iss";
    }

    public class YahooFinanceSettings
    {
        public string BaseUrl { get; set; } = "https://query1.finance.yahoo.com/v8/finance/chart";
        public int RequestDelayMs { get; set; } = 1000;
    }
}
