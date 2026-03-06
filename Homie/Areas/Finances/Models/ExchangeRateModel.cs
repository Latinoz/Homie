using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Курс валюты к базовой (RUB) на дату</summary>
    public class ExchangeRateModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Валюта")]
        public int CurrencyId { get; set; }
        public CurrencyModel Currency { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        /// <summary>Курс к базовой валюте (RUB)</summary>
        [Required]
        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Курс")]
        public decimal Rate { get; set; }

        [Display(Name = "Источник")]
        public PriceSource Source { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }
    }
}
