using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Homie.Areas.Series.Models;
using Homie.Areas.Cigars.Models;
using Homie.Areas.Battletech.Models;

namespace Homie.Models
{
    public class IndexViewModel
    {
        public IEnumerable<MoviesModel> Series { get; set; }
        public IEnumerable<CigarsModel> Cigars { get; set; }
        public IEnumerable<BTMechsModel> Mechs { get; set; }
        public IEnumerable<BTPilotsModel> Pilots { get; set; }
        public IEnumerable<Image> Images { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterViewModel FilterViewModel { get; set; }
        public SortViewModel SortViewModel { get; set; }
    }
}
