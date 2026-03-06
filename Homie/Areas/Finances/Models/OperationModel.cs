using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Операция (транзакция) в журнале</summary>
    public class OperationModel
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        [Display(Name = "Тип операции")]
        public int OperationTypeId { get; set; }
        public OperationTypeModel OperationType { get; set; }

        [Display(Name = "Счёт")]
        public int AccountId { get; set; }
        public AccountModel Account { get; set; }

        [Display(Name = "Инструмент")]
        public int? InstrumentId { get; set; }
        public InstrumentModel Instrument { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Количество")]
        public decimal? Quantity { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Цена")]
        public decimal? Price { get; set; }

        [Display(Name = "Валюта")]
        public int CurrencyId { get; set; }
        public CurrencyModel Currency { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        [Display(Name = "Курс к RUB")]
        public decimal ExchangeRateToRub { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Сумма (RUB)")]
        public decimal AmountInRub { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Комиссия (RUB)")]
        public decimal? Commission { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Налог (RUB)")]
        public decimal? Tax { get; set; }

        [Column(TypeName = "varchar(500)")]
        [Display(Name = "Заметки")]
        public string Notes { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }
    }
}
