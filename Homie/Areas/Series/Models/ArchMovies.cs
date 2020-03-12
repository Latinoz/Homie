using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Homie.Areas.Series.Models
{
    public class ArchMovies
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public int StopPlayMinute { get; set; }
        public int StopPlaySecond { get; set; }
    }
}
