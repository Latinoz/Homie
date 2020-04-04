using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Homie.Areas.Cigars.Models
{
    public class CigarsModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Length { get; set; }
        public string Ring { get; set; }
        public string Country { get; set; }
        public string Filler { get; set; }
        public string Wrapper { get; set; }
        public string Color { get; set; }
        public string Strength { get; set; }
        public string Shape { get; set; }
        public int Rating { get; set; }
        public int Price { get; set; }
        public string Brand { get; set; }

    }
}
