using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Позиция в инвестиционном портфеле (акции, облигации, ETF)</summary>
    public class InvestmentPositionModel
    {
        public int Id { get; set; }

        [Display(Name = "Инструмент")]
        public int InstrumentId { get; set; }
        public InstrumentModel Instrument { get; set; }

        [Display(Name = "Брокерский счёт")]
        public int AccountId { get; set; }
        public AccountModel Account { get; set; }

        /// <summary>Количество (ручной ввод)</summary>
        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Количество")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Средняя цена покупки")]
        public decimal AvgPurchasePrice { get; set; }

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
        public string UserUid { get; set; }

        // --- Вычисляемые свойства ---

        [NotMapped]
        [Display(Name = "Кол-во (журнал)")]
        public decimal QuantityFromJournal { get; set; }

        [NotMapped]
        [Display(Name = "Ср. цена (журнал)")]
        public decimal AvgPriceFromJournal { get; set; }

        [NotMapped]
        [Display(Name = "Дивиденды (журнал)")]
        public decimal DividendsFromJournal { get; set; }

        [NotMapped]
        [Display(Name = "Доходность, %")]
        public decimal ReturnPercent { get; set; }

        [NotMapped]
        [Display(Name = "Стоимость (RUB)")]
        public decimal ValueInRub { get; set; }
    }
}
