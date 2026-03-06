using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Позиция в драгметаллах (золото, серебро, платина, палладий)</summary>
    public class PreciousMetalModel
    {
        public int Id { get; set; }

        [Display(Name = "Металл")]
        public MetalType Metal { get; set; }

        /// <summary>Описание (напр. "Монета Георгий Победоносец")</summary>
        [Required]
        [Column(TypeName = "varchar(255)")]
        [Display(Name = "Название / Описание")]
        public string Name { get; set; }

        /// <summary>Проба (999, 585 и т.д.)</summary>
        [Column(TypeName = "varchar(10)")]
        [Display(Name = "Проба")]
        public string Purity { get; set; }

        [Column(TypeName = "decimal(10,3)")]
        [Display(Name = "Вес, г")]
        public decimal WeightGrams { get; set; }

        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        /// <summary>Цена покупки за единицу в RUB</summary>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Цена покупки (₽/шт)")]
        public decimal PurchasePrice { get; set; }

        /// <summary>Дата покупки</summary>
        [DataType(DataType.Date)]
        [Display(Name = "Дата покупки")]
        public DateTime? PurchaseDate { get; set; }

        /// <summary>Актуальная цена ЦБ за грамм (авто или ручной ввод)</summary>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Цена ЦБ (₽/г)")]
        public decimal CurrentPricePerGram { get; set; }

        [Display(Name = "Автообновление цены")]
        public bool IsAutoUpdateEnabled { get; set; } = true;

        [Display(Name = "Дата ручной корректировки")]
        public DateTime? LastManualOverrideDate { get; set; }

        [Column(TypeName = "varchar(500)")]
        [Display(Name = "Заметки")]
        public string Notes { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }

        // --- Вычисляемые ---

        [NotMapped]
        [Display(Name = "Общий вес, г")]
        public decimal TotalWeightGrams => WeightGrams * Quantity;

        [NotMapped]
        [Display(Name = "Стоимость (RUB)")]
        public decimal TotalValueRub => TotalWeightGrams * CurrentPricePerGram;
    }
}
