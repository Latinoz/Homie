using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Криптоактив (BTC, ETH и др.)</summary>
    public class CryptoAssetModel
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Выберите инструмент")]
        [Display(Name = "Инструмент")]
        public int InstrumentId { get; set; }
        public InstrumentModel Instrument { get; set; }

        [Display(Name = "Счёт / Биржа")]
        public int? AccountId { get; set; }
        public AccountModel Account { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        [Display(Name = "Тикер")]
        public string Ticker { get; set; }

        [Column(TypeName = "varchar(50)")]
        [Display(Name = "CoinGecko ID")]
        public string CoinGeckoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Выберите валюту")]
        [Display(Name = "Валюта оценки")]
        public int CurrencyId { get; set; }
        public CurrencyModel Currency { get; set; }

        /// <summary>Количество (ручной ввод)</summary>
        [Column(TypeName = "decimal(18,8)")]
        [Display(Name = "Количество")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Средняя цена покупки")]
        public decimal AvgPurchasePrice { get; set; }

        /// <summary>Текущая цена (авто CoinGecko или ручной ввод)</summary>
        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Текущая цена")]
        public decimal CurrentPrice { get; set; }

        [Display(Name = "Автообновление цены")]
        public bool IsAutoUpdateEnabled { get; set; } = true;

        [Display(Name = "Дата ручной корректировки")]
        public DateTime? LastManualOverrideDate { get; set; }

        [Column(TypeName = "varchar(500)")]
        [Display(Name = "Заметки")]
        public string Notes { get; set; }

        [Column(TypeName = "varchar(255)")]
        [Display(Name = "Адрес кошелька")]
        public string WalletAddress { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }

        // --- Вычисляемые ---

        [NotMapped]
        [Display(Name = "Кол-во (журнал)")]
        public decimal QuantityFromJournal { get; set; }

        [NotMapped]
        [Display(Name = "Стоимость (USD)")]
        public decimal ValueInUsd { get; set; }

        [NotMapped]
        [Display(Name = "Стоимость (RUB)")]
        public decimal ValueInRub { get; set; }
    }
}
