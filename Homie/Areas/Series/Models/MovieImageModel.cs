using Homie.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Homie.Areas.Series.Models
{
    public class MovieImageModel : ImageViewModel
    {
        [TempData]
        public int tempIdMovie { get; set; }

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

        [TempData]
        public DateTime? tempHoldPlay { get; set; }
    }
}
