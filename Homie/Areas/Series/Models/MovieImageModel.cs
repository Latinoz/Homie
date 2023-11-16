using Homie.Models;
using Microsoft.AspNetCore.Mvc;


namespace Homie.Areas.Series.Models
{
    public class MovieImageModel : ImageViewModel
    {
        [TempData]
        public string? tempName { get; set; }

        [TempData]
        public string? tempLink { get; set; }

        [TempData]
        public string? tempCategory { get; set; }

        [TempData]
        public int? tempSeason { get; set; }

        [TempData]
        public int? tempEpisode { get; set; }
    }
}
