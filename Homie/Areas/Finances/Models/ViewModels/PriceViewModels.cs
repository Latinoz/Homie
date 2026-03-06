using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Homie.Areas.Finances.Models
{
    /// <summary>Результат обновления цен</summary>
    public class PriceUpdateResultViewModel
    {
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int SkippedCount { get; set; }
        public List<PriceUpdateItemResult> Items { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
    }

    public class PriceUpdateItemResult
    {
        public string InstrumentName { get; set; }
        public string Ticker { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? NewPrice { get; set; }
        public PriceSource Source { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public bool Skipped { get; set; }
        public string SkipReason { get; set; }
    }

    /// <summary>Форма ручного ввода / корректировки цены</summary>
    public class ManualPriceEntryViewModel
    {
        [Required]
        [Display(Name = "Инструмент")]
        public int InstrumentId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Валюта")]
        public int CurrencyId { get; set; }

        /// <summary>Обновить также текущую цену на позиции</summary>
        [Display(Name = "Обновить текущую цену")]
        public bool UpdateCurrentPrice { get; set; } = true;
    }

    /// <summary>Данные для графика истории цены инструмента</summary>
    public class PriceHistoryChartViewModel
    {
        public int InstrumentId { get; set; }
        public string InstrumentName { get; set; }
        public string Ticker { get; set; }

        /// <summary>JSON-данные для Chart.js</summary>
        public string ChartDataJson { get; set; }
    }
}
