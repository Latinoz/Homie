using Homie.Models;
using Microsoft.AspNetCore.Mvc;


namespace Homie.Areas.Series.Models
{
    public class MovieImageModel : ImageViewModel
    {
        [TempData]
        public string? tempName { get; set; }

        [TempData]
        public int? tempLink { get; set; }

        [TempData]
        public int? tempCategory { get; set; }

        [TempData]
        public string? tempSeason { get; set; }

        [TempData]
        public string? tempEpisode { get; set; }
    }
}
