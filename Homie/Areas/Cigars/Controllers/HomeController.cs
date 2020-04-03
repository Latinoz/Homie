using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Homies.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Homie.Areas.Cigars.Controllers
{
    [Area("Cigars")]
    [Authorize(Roles = "admin,user")]
    public class HomeController : Controller
    {
        ApplicationDbContext db;

        public HomeController(ApplicationDbContext context)
        {
            db = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await db.Cigars.ToListAsync());
        }
    }
}