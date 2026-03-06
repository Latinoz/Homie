using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Тип операции (seed data — 15 предустановленных)</summary>
    public class OperationTypeModel
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Категория")]
        public OperationCategory Category { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }
    }
}
