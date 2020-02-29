using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Homie.Areas.Series.Models;

namespace Homie.Areas.Series.Controllers
{
    [Area("Series")]
    public class HomeController : Controller
    {
        private Movies[] data = new Movies[] {
            new Movies { Name = "Serial 1", Season = 1, Episode = 1 },
            new Movies { Name = "Serial 2", Season = 3, Episode = 14 },
            new Movies { Name = "Serial 3", Season = 10, Episode = 2 }
        };

        public ViewResult Index() => View(data);
    }
}