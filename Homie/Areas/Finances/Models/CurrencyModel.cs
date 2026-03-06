using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Валюта (RUB, USD, EUR, CNY, GBP и др.)</summary>
    public class CurrencyModel
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(10)")]
        [Display(Name = "Код")]
        public string Code { get; set; }

        [Required]
        [Column(TypeName = "varchar(255)")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        /// <summary>Код ЦБ РФ, напр. R01235 для USD</summary>
        [Column(TypeName = "varchar(20)")]
        [Display(Name = "Код ЦБ")]
        public string CbrCode { get; set; }

        [Display(Name = "Базовая валюта")]
        public bool IsBase { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }
    }
}
