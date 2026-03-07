using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Банк / Брокер (справочник)</summary>
    public class BankModel
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(255)")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }
    }
}
