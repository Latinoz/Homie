using Homie.Models;
using Microsoft.AspNetCore.Mvc;


namespace Homie.Areas.Series.Models
{
    public class MovieImageModel : ImageViewModel
    {
        [TempData]
        public string? tempName { get; set; }

        [TempData]
        public int? tempTonnage { get; set; }

        [TempData]
        public int? tempBV { get; set; }

        [TempData]
        public string? tempTypeMech { get; set; }

        [TempData]
        public string? tempUidMech { get; set; }

        [TempData]
        public int? tempIdMech { get; set; }

    }
}
