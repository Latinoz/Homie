using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Homie.Areas.Series.Models;

namespace Homie.Models
{
    public class IndexViewModel
    {
        public IEnumerable<MoviesModel> Series { get; set; }
        public PageViewModel PageViewModel { get; set; }
    }
}
