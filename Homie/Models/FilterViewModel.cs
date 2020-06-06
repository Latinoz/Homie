using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Homie.Areas.Cigars.Models;

namespace Homie.Models
{
    public class FilterViewModel
    {
        public FilterViewModel(List<Format> formats, int? format, string name)
        {
            // устанавливаем начальный элемент, который позволит выбрать всех
            formats.Insert(0, new Format { ShapeName = "Все", Id = 0 });
            Formats = new SelectList(formats, "Id", "ShapeName", format);
            SelectedFormat = format;
            SelectedName = name;
        }
        public SelectList Formats { get; private set; } // список форматов сигар
        public int? SelectedFormat { get; private set; }   // выбранный формат
        public string SelectedName { get; private set; }    // введенное имя
    }
}
