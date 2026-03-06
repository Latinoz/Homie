using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Данные инфляции — месячный ИПЦ</summary>
    public class InflationModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Год")]
        public int Year { get; set; }

        [Required]
        [Range(1, 12)]
        [Display(Name = "Месяц")]
        public int Month { get; set; }

        /// <summary>ИПЦ за месяц, %</summary>
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "ИПЦ, %")]
        public decimal CpiPercent { get; set; }

        /// <summary>Накопленная инфляция за год, %</summary>
        [Column(TypeName = "decimal(8,4)")]
        [Display(Name = "Накопленная за год, %")]
        public decimal? AccumulatedYearPercent { get; set; }

        [Column(TypeName = "varchar(500)")]
        [Display(Name = "Заметки")]
        public string Notes { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }
    }
}
