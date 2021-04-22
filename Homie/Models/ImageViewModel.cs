using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Homie.Models
{
    public class ImageViewModel
    {
        public string Name { get; set; }
        public IFormFile Avatar { get; set; }
    }
}
