using Homie.Areas.Battletech.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Homie.Models
{
    public class ImageViewModel
    {
        public string NameImgVM { get; set; }
        public IFormFile AvatarFile { get; set; }

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
