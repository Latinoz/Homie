using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Финансовый инструмент (тикер/ISIN) — акции, облигации, ETF, крипто</summary>
    public class InstrumentModel
    {
        public int Id { get; set; }

        /// <summary>Внутренний код / тикер (SBER, AAPL, BTC)</summary>
        [Required]
        [Column(TypeName = "varchar(50)")]
        [Display(Name = "Код / Тикер")]
        public string Code { get; set; }

        [Required]
        [Column(TypeName = "varchar(255)")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Тип")]
        public InstrumentType Type { get; set; }

        [Display(Name = "Биржа")]
        public Exchange Exchange { get; set; }

        /// <summary>ISIN (International Securities Identification Number)</summary>
        [Column(TypeName = "varchar(20)")]
        [Display(Name = "ISIN")]
        public string ISIN { get; set; }

        /// <summary>Код на внешнем источнике, если отличается от Code (напр. coingecko id "bitcoin")</summary>
        [Column(TypeName = "varchar(50)")]
        [Display(Name = "Внешний код")]
        public string ExternalCode { get; set; }

        [Display(Name = "Валюта")]
        public int CurrencyId { get; set; }
        public CurrencyModel Currency { get; set; }

        [Column(TypeName = "varchar(100)")]
        [Display(Name = "Категория")]
        public string Category { get; set; }

        /// <summary>Кэш последней цены</summary>
        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Последняя цена")]
        public decimal? LastPrice { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата обновления цены")]
        public System.DateTime? LastPriceDate { get; set; }

        [Display(Name = "Источник цены")]
        public PriceSource? LastPriceSource { get; set; }

        [Column(TypeName = "varchar(500)")]
        [Display(Name = "Заметки")]
        public string Notes { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }
    }
}
