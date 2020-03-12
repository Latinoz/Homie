using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Homies.Models;
using Homie.Areas.Series.Models;

namespace Homie.Areas.Series.Controllers
{
    [Area("Series")]
    public class HomeController : Controller
    {
        ApplicationDbContext db;

        public HomeController(ApplicationDbContext context)
        {
            db = context;
        }
        
        public ViewResult Index() => View(db.Movies.ToList());
        
    }
}