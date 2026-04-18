using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Тип инструмента (справочник) — Акции, Облигации, ETF, Криптовалюта, Драгметаллы, Валюта и т.д.</summary>
    public class InvestmentTypeModel
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        /// <summary>Системный код (Stock, Bond, ETF, Crypto, PreciousMetal, Currency). Null для пользовательских типов.</summary>
        [Column(TypeName = "varchar(50)")]
        [Display(Name = "Системный код")]
        public string SystemCode { get; set; }

        /// <summary>Null = глобальный (системный) тип, иначе — пользовательский</summary>
        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }
    }
}
