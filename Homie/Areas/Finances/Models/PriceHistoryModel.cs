using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>История цен инструментов (для графиков динамики)</summary>
    public class PriceHistoryModel
    {
        public int Id { get; set; }

        [Display(Name = "Инструмент")]
        public int InstrumentId { get; set; }
        public InstrumentModel Instrument { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [Display(Name = "Валюта")]
        public int CurrencyId { get; set; }
        public CurrencyModel Currency { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Цена (RUB)")]
        public decimal PriceInRub { get; set; }

        [Display(Name = "Источник")]
        public PriceSource Source { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }
    }
}
