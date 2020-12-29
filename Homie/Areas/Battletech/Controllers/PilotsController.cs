using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Homie.Areas.Battletech.Controllers
{
    public class PilotsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
