using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Депозит / накопительный счёт</summary>
    public class DepositModel
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(255)")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Счёт")]
        public int AccountId { get; set; }
        public AccountModel Account { get; set; }

        [Display(Name = "Валюта")]
        public int CurrencyId { get; set; }
        public CurrencyModel Currency { get; set; }

        /// <summary>Сумма вклада (ручной ввод)</summary>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Сумма")]
        public decimal Amount { get; set; }

        /// <summary>Процентная ставка</summary>
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Ставка, %")]
        public decimal InterestRate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата открытия")]
        public DateTime OpenDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата закрытия")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Капитализация")]
        public bool IsCapitalization { get; set; }

        [Column(TypeName = "varchar(500)")]
        [Display(Name = "Заметки")]
        public string Notes { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }

        // --- Вычисляемые свойства (из журнала операций + курсов) ---

        [NotMapped]
        [Display(Name = "Баланс (журнал)")]
        public decimal BalanceFromJournal { get; set; }

        [NotMapped]
        [Display(Name = "Начисленные проценты")]
        public decimal AccruedInterest { get; set; }

        [NotMapped]
        [Display(Name = "Стоимость (RUB)")]
        public decimal ValueInRub { get; set; }
    }
}
