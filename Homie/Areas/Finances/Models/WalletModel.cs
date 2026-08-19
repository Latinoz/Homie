using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Кошелёк (справочник)</summary>
    public class WalletModel
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(255)")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Тип кошелька")]
        public WalletType Type { get; set; } = WalletType.Fiat;

        [Display(Name = "Валюта")]
        public int? CurrencyId { get; set; }
        public CurrencyModel Currency { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }
    }
}
