﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Homie.Areas.Cigars.Models
{
    public class CigarFilterViewModel
    {
        public CigarFilterViewModel(List<Format> formats, int? format, string name)
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
